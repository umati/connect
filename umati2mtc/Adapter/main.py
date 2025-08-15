# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

"""
Main entry point for the UMATI OPC UA to MTConnect adapter.

This module orchestrates the MQTT client, message processing, and SHDR server
to bridge OPC UA umati data to MTConnect format.
"""

import asyncio
import os
import sys
from queue import Queue

# Helper for configuration, mapping and the XML state
from config_store import ConfigStore
from services.create_mapping import load_mapping
# Background services
from services.mqtt_client import start_mqtt
from services.process_queue import process_queue
from services.send_shdr import start_shdr_server

# Environment variables for MQTT and SHDR server configuration
BROKER_IP = os.getenv("MQTT_BROKER_IP", "mosquitto")
BROKER_PORT = int(os.getenv("MQTT_BROKER_PORT", "1883"))
TOPIC_PREFIX = os.getenv("MQTT_TOPIC_PREFIX", "umati/v2/ifw/MachineToolType/#")
SHDR_IP = os.getenv("SHDR_SERVER_IP", "127.0.0.1")
SHDR_PORT = int(os.getenv("SHDR_SERVER_PORT", "7878"))

shutdown_event = asyncio.Event()  # Used to signal shutdown


def load_config():
    """Load configuration and mapping data from files."""
    try:
        ConfigStore.load_config_json("mazak")
        mapped_objects = load_mapping(
            ConfigStore.Mapping_file, ConfigStore.Mapping_sheet
        )
        print(
            f"[INFO] Loaded {len(mapped_objects)} mapped objects from {ConfigStore.Mapping_file}."
        )
        return mapped_objects
    except (FileNotFoundError, KeyError, ValueError) as e:
        print(f"[ERROR] Configuration error: {e}")
        return None


async def main():
    """Main application loop orchestrating MQTT, processing, and SHDR services."""
    # Load configuration and mappings
    mapped_objects = load_config()

    # Create a queue for MQTT messages
    data_queue = Queue()

    # Start MQTT client and put messages into data_queue
    start_mqtt(BROKER_IP, BROKER_PORT, TOPIC_PREFIX, data_queue)

    # Process the queue of MQTT messages and update mapped_objects with values
    task_process_queue = asyncio.create_task(process_queue(data_queue, mapped_objects))

    # Start the SHDR server to send data to the MTConnect agent
    task_shdr_server = asyncio.create_task(
        start_shdr_server(SHDR_IP, SHDR_PORT, mapped_objects)
    )

    print("Adapter initialized. Press Ctrl+C to exit.")

    try:
        while True:
            await asyncio.sleep(1)
    except KeyboardInterrupt:
        print("Shutdown requested. Cancelling tasks...")
        task_process_queue.cancel()
        await asyncio.gather(
            task_process_queue, task_shdr_server, return_exceptions=True
        )
        print("Shutdown complete.")


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except (FileNotFoundError, KeyError, ValueError, ConnectionError) as e:
        print(f"Error during execution: {e}")
        shutdown_event.set()
        print("Exiting due to error.")
        sys.exit(1)
    except KeyboardInterrupt:
        print("Ctrl+C received. Shutting down gracefully...")
        shutdown_event.set()
        sys.exit(0)
