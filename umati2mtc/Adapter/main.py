# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover. All rights reserved.

import asyncio
from queue import Queue

# Helper for configuration, mapping and the XML state
from config_store import ConfigStore
from services.create_mappings import load_mapping

# Background services
from services.mqtt_client import start_mqtt
from services.process_queue import process_queue
from services.send_shdr import start_shdr_server

shutdown_event = asyncio.Event()  # Used to signal shutdown

def load_config():
    try:
        ConfigStore.load_config_json("mazak_SR2")
        mapped_objects = load_mapping(
            ConfigStore.Mapping_file, ConfigStore.Mapping_sheet
        )
        print(f"[INFO] Loaded {len(mapped_objects)} mapped objects from {ConfigStore.Mapping_file}.")
        return mapped_objects
    except Exception as e:
        print(f"[ERROR] Configuration error: {e}")
        return None
    

async def main():

    mapped_objects = load_config()  # Load configuration and mappings

    data_queue = Queue()  # Create a queue for MQTT messages

    # Start background tasks
    # Start MQTT client and put messages into data_queue
    start_mqtt(
        ConfigStore.MQTTServerIP,
        ConfigStore.MQTTPort,
        ConfigStore.Gateway_topic_prefix,
        data_queue,
    )

    #await asyncio.sleep(5)
    task_process_queue = asyncio.create_task(process_queue(data_queue, mapped_objects)) # Process the queue of MQTT messages and update mapped_objects with values
    task_shdr_server = asyncio.create_task(start_shdr_server(mapped_objects)) # Start the SHDR server to send data to the MTConnect agent

    print("Adapter initialized. Press Ctrl+C to exit.")

    try:
        while True:
            await asyncio.sleep(1)
    except KeyboardInterrupt:
        print("Shutdown requested. Cancelling tasks...")
        task_process_queue.cancel()
        await asyncio.gather(task_process_queue, task_shdr_server, return_exceptions=True)
        print("Shutdown complete.")

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except Exception as e:
        print(f"Error during execution: {e}")
        shutdown_event.set()
        print("Exiting due to error.")
        exit(1)
    except KeyboardInterrupt:
        print("Ctrl+C received. Shutting down gracefully...")
        shutdown_event.set()
        exit(0)
