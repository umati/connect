# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover. All rights reserved.

import asyncio
import datetime

async def handle_connection(mapped_objects, reader, writer):
    while True:
        try:
            now = datetime.datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%fZ")
        
            # Build SHDR message with all available specname-value pairs
            shdr_parts = [now]
            for mapped_object in mapped_objects:
                if mapped_object.value is not None and mapped_object.mtc_specname is not None:
                    shdr_parts.append(f"{mapped_object.mtc_specname}|{mapped_object.value}")
            
            # Skip if there's no data
            if len(shdr_parts) == 1:
                await asyncio.sleep(1)
                continue

            message = '|'.join(shdr_parts) + '\n'
            writer.write(message.encode())
            print(f"[SHDR Sent] {message.strip()}")
            await writer.drain()
            
            await asyncio.sleep(1)  # Adjustable send rate
        except Exception as e:
            print(f"[SHDR Error] {e}")
            print("[SHDR] Retrying in 10 seconds...")
            await asyncio.sleep(10)  # Wait before retrying
            pass

async def start_shdr_server(shdr_server_ip, shdr_server_port, mapped_objects):
    try:
        server = await asyncio.start_server(
        lambda r, w: handle_connection(mapped_objects, r, w),
        host=shdr_server_ip,
        port=shdr_server_port
        )
        print("SHDR Adapter running on {}:{}".format(shdr_server_ip, shdr_server_port))
        async with server:
            await server.serve_forever()
    except Exception as e:
        print(f"[SHDR Server Error] {e}")
        print("Failed to start SHDR server. Retrying in 10 seconds...")
        await asyncio.sleep(10)


