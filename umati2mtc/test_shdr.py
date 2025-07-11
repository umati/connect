import asyncio
import datetime

async def handle_connection(reader, writer):
    feed = 0
    while True:
        now = datetime.datetime.utcnow().isoformat() + "Z"
        message = f"{now}|Fact|{feed}|Fovr|{feed}\n"
        writer.write(message.encode())
        feed += 1
        print(f"Sent: {message.strip()}")
        await writer.drain()
        await asyncio.sleep(3)

async def start_shdr_server():
    server = await asyncio.start_server(handle_connection, host='127.0.0.1', port=7878)
    print("SHDR Adapter running on 127.0.0.1:7878")
    async with server:
        await server.serve_forever()

if __name__ == "__main__":
    asyncio.run(start_shdr_server())
