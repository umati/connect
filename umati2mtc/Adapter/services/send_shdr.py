# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

"""
SHDR (Simple Hierarchical Data Representation) server for MTConnect communication.

This module implements the SHDR protocol server to send data to the MTConnect agent.
"""

import asyncio
import datetime
import random


async def handle_connection(mapped_objects, _reader, writer):
    """Handle SHDR client connection and send formatted data messages."""
    while True:
        try:
            now = datetime.datetime.now(
                datetime.timezone(datetime.timedelta(hours=2))
            ).strftime("%Y-%m-%dT%H:%M:%S.%fZ")
            # Build SHDR message with all available specname-value pairs
            shdr_parts = [now]
            for mapped_object in mapped_objects:
                if (
                    mapped_object.value is not None
                    and mapped_object.mtc_specname is not None
                ):
                    shdr_parts.append(
                        f"{mapped_object.mtc_specname}|{mapped_object.value}"
                    )

            # Add mandatory data for compatibility with Mazak Smooth Monitor Ax
            shdr_parts.append("avail|AVAILABLE")
            shdr_parts.append("functionalmode|PRODUCTION")

            # Add additional simulated data
            shdr_parts = await simulate_complete_data(shdr_parts)

            # Skip if there's no data
            if len(shdr_parts) == 1:
                await asyncio.sleep(1)
                continue

            message = "|".join(shdr_parts) + "\n"
            writer.write(message.encode())
            print(f"[SHDR Sent] {message.strip()}")
            await writer.drain()

            await asyncio.sleep(1)  # Adjustable send rate
        except (ConnectionResetError, BrokenPipeError, OSError) as e:
            print(f"[SHDR Error] {e}")
            print("[SHDR] Retrying in 10 seconds...")
            await asyncio.sleep(10)  # Wait before retrying


async def start_shdr_server(shdr_server_ip, shdr_server_port, mapped_objects):
    """Start SHDR server to send data to MTConnect agent."""
    try:
        server = await asyncio.start_server(
            lambda r, w: handle_connection(mapped_objects, r, w),
            host=shdr_server_ip,
            port=shdr_server_port,
        )
        print(f"SHDR Adapter running on {shdr_server_ip}:{shdr_server_port}")
        async with server:
            await server.serve_forever()
    except (OSError, ValueError) as e:
        print(f"[SHDR Server Error] {e}")
        print("Failed to start SHDR server. Retrying in 10 seconds...")
        await asyncio.sleep(10)


async def simulate_complete_data(shdr_parts):
    """Simulate (non-UA4MT) data for better compatibility with Mazak Smooth Monitor Ax."""

    # ---- AXES POSITIONS, LOADS, FEEDRATES, TEMPS ----
    for axis in ["X", "Y", "Z"]:
        shdr_parts.append(f"{axis}abs|{round(random.uniform(-500, 500), 3)}")
        shdr_parts.append(f"{axis}pos|{round(random.uniform(-500, 500), 3)}")
        shdr_parts.append(f"{axis}load|{random.randint(0, 120)}")
        shdr_parts.append(f"{axis}frt|{round(random.uniform(100, 1000), 2)}")
        shdr_parts.append(
            f"servotemp{['X', 'Y', 'Z'].index(axis) + 1}|{round(random.uniform(30, 80), 1)}"
        )
        shdr_parts.append(
            f"{axis.lower()}axisstate|{random.choice(['ACTIVE', 'HOME', 'STOPPED'])}"
        )

    # Rotary B axis
    shdr_parts.append(f"Babs|{round(random.uniform(0, 360), 2)}")
    shdr_parts.append(f"Bpos|{round(random.uniform(0, 360), 2)}")
    shdr_parts.append(f"Bload|{random.randint(0, 120)}")
    shdr_parts.append(f"Bfrt|{round(random.uniform(1, 90), 2)}")
    shdr_parts.append(f"arfunc|{random.choice(['CONTOUR', 'INDEX'])}")
    shdr_parts.append(f"baxisstate|{random.choice(['ACTIVE', 'HOME', 'STOPPED'])}")

    # Rotary C axis
    shdr_parts.append(f"Cabs|{round(random.uniform(0, 360), 2)}")
    shdr_parts.append(f"Cpos|{round(random.uniform(0, 360), 2)}")
    shdr_parts.append(f"Cload|{random.randint(0, 120)}")
    shdr_parts.append(f"Sload|{random.randint(0, 120)}")
    shdr_parts.append(f"Cfrt|{round(random.uniform(1, 90), 2)}")
    shdr_parts.append(f"Srpm|{random.randint(0, 5000)}")
    shdr_parts.append(f"Stemp|{round(random.uniform(30, 90), 1)}")
    shdr_parts.append(f"crfunc|{random.choice(['SPINDLE', 'INDEX'])}")
    shdr_parts.append(f"caxisstate|{random.choice(['ACTIVE', 'HOME', 'STOPPED'])}")

    # ---- CONTROLLER ----
    shdr_parts.append(f"estop|{random.choice(['ARMED', 'TRIGGERED'])}")
    shdr_parts.append(f"pallet_num|{random.randint(1, 10)}")
    shdr_parts.append(f"fixtureid|FIX{random.randint(100, 999)}")
    shdr_parts.append(f"Frapidovr|{random.randint(0, 120)}")
    shdr_parts.append(f"Fovr|{random.randint(0, 120)}")
    shdr_parts.append(f"Sovr|{random.randint(0, 120)}")
    # shdr_parts.append(f"program|O{random.randint(1000, 9999)}")
    shdr_parts.append(f"activeprog|O{random.randint(1000, 9999)}")
    shdr_parts.append(f"PartCountAct|{random.randint(0, 100)}")
    shdr_parts.append(f"PartCountTarget|{random.randint(100, 200)}")
    shdr_parts.append(f"Fact|{round(random.uniform(100, 2000), 2)}")
    shdr_parts.append(f"Tool_number|{random.randint(1, 30)}")
    shdr_parts.append(f"Tool_group|G{random.randint(1, 10)}")
    shdr_parts.append(
        f"execution|{random.choice(['READY', 'ACTIVE', 'STOPPED', 'INTERRUPTED', 'COMPLETED'])}"
    )
    shdr_parts.append(f"waitstate|{random.choice(['NONE', 'WAITING'])}")
    shdr_parts.append(f"mode|{random.choice(['AUTOMATIC', 'MANUAL', 'MDI'])}")
    shdr_parts.append(f"linelabel|LBL{random.randint(1, 50)}")
    shdr_parts.append(f"linenumber|{random.randint(1, 1000)}")
    shdr_parts.append(f"cspeed|{round(random.uniform(50, 200), 2)}")
    shdr_parts.append(
        f"workoffsettrans|"
        f"{round(random.uniform(-10, 10), 2)} "
        f"{round(random.uniform(-10, 10), 2)} "
        f"{round(random.uniform(-10, 10), 2)}"
    )
    shdr_parts.append(
        f"workoffsetrot|"
        f"{round(random.uniform(-5, 5), 2)} "
        f"{round(random.uniform(-5, 5), 2)} "
        f"{round(random.uniform(-5, 5), 2)}"
    )
    shdr_parts.append(
        f"pathpos|"
        f"{round(random.uniform(0, 500), 2)} "
        f"{round(random.uniform(0, 500), 2)} "
        f"{round(random.uniform(0, 500), 2)}"
    )
    shdr_parts.append(
        f"orientation|"
        f"{round(random.uniform(0, 360), 2)} "
        f"{round(random.uniform(0, 360), 2)} "
        f"{round(random.uniform(0, 360), 2)}"
    )
    shdr_parts.append(f"proctimer|{random.randint(0, 5000)}")

    # ---- DOOR & SYSTEMS ----
    shdr_parts.append(f"doorstate|{random.choice(['OPEN', 'CLOSED'])}")
    shdr_parts.append(f"cooltemp|{round(random.uniform(20, 40), 1)}")
    shdr_parts.append(f"CONCENTRATION|{random.randint(5, 15)}")
    shdr_parts.append(f"rmtmp1|{round(random.uniform(18, 30), 1)}")

    # ---- RESOURCES ----
    shdr_parts.append(f"stock|STOCK-{random.randint(100, 999)}")

    return shdr_parts
