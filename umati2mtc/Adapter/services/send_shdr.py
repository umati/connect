# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

"""
SHDR (Simple Hierarchical Data Representation) server for MTConnect communication.

This module implements the SHDR protocol server to send data to the MTConnect agent.
"""

import asyncio
import datetime


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
