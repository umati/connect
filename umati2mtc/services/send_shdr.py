import asyncio
import datetime

async def handle_connection(mapped_objects, reader, writer):
    while True:
        now = datetime.datetime.utcnow().isoformat() + "Z"
        
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



async def start_shdr_server(mapped_objects):
    server = await asyncio.start_server(
        lambda r, w: handle_connection(mapped_objects, r, w),
        host='127.0.0.1',
        port=7878
    )
    print("SHDR Adapter running on 127.0.0.1:7878")
    async with server:
        await server.serve_forever()


