# ========================================================================
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
# ========================================================================

import asyncio
import threading
from queue import Queue

# Flask app
from app import create_app

# Helper for configuration, mapping and the XML state
from helper.config_store import ConfigStore
from helper.xml_state import initialize_xml_state
from services.create_mappings import load_mapping

# Background services
from services.mqtt_client import start_mqtt
from services.process_queue import process_queue
from services.send_shdr import start_shdr_server
from services.update_mtc_values import update_xml_with_values

shutdown_event = asyncio.Event()  # Used to signal shutdown


def load_config():
    try:
        ConfigStore.load_config_json(
            "mazak_SR"
        )  # ConfigStore.load_config_json("mazak_OS")
        mapped_objects = load_mapping(
            ConfigStore.Mapping_file, ConfigStore.Mapping_sheet
        )
        print(
            f"[INFO] Loaded {len(mapped_objects)} mapped objects from {ConfigStore.Mapping_file}."
        )
        ConfigStore.load_config_json("mazak_SR2")
        mapped_objects = load_mapping(ConfigStore.Mapping_file, ConfigStore.Mapping_sheet)
        print(f"[INFO] Loaded {len(mapped_objects)} mapped objects from {ConfigStore.Mapping_file}.")
        return mapped_objects
    except Exception as e:
        print(f"[ERROR] Configuration error: {e}")
        return None
    

async def main():

    mapped_objects = load_config()  # Load configuration and mappings

    data_queue = Queue()  # Create a queue for MQTT messages

    xml_state = initialize_xml_state(
        ConfigStore.Information_model
    )  # Initialize XML state with the information model
    if xml_state is None:
        print("[ERROR] Failed to initialize XML state. Exiting.")
        return

    app = create_app(xml_state)
    flask_thread = threading.Thread(
        target=lambda: app.run(
            host=ConfigStore.MTConnectServerIP, port=ConfigStore.MTConnectServerPort
        ),
        daemon=True,
    )
    flask_thread.start()  # Start Flask app in a separate thread

    # Start background tasks
    start_mqtt(
        ConfigStore.MQTTServerIP,
        ConfigStore.MQTTPort,
        ConfigStore.Gateway_topic_prefix,
        data_queue,
    )  # Start MQTT client and put messages into data_queue
    await asyncio.sleep(5)
    task_process_queue = asyncio.create_task(process_queue(data_queue, mapped_objects))
    task_update_xml = asyncio.create_task(
        update_xml_with_values(
            mapped_objects,
            xml_state,
            ConfigStore.DevicestreamName,
            ConfigStore.MTConnectNamespace,
        )
    )
    start_mqtt(ConfigStore.MQTTServerIP, ConfigStore.MQTTPort, ConfigStore.Gateway_topic_prefix, data_queue) # Start MQTT client and put messages into data_queue
    #await asyncio.sleep(5)
    task_process_queue = asyncio.create_task(process_queue(data_queue, mapped_objects)) # Process the queue of MQTT messages and update mapped_objects with values
    asyncio.create_task(start_shdr_server(mapped_objects))

    #task_update_xml = asyncio.create_task(update_xml_with_values(mapped_objects, xml_state, ConfigStore.DevicestreamName, ConfigStore.MTConnectNamespace)) # deprecated
    


    print("Adapter initialized. Press Ctrl+C to exit.")

    try:
        while True:
            await asyncio.sleep(1)
    except KeyboardInterrupt:
        print("Shutdown requested. Cancelling tasks...")
        task_process_queue.cancel()
        task_update_xml.cancel()
        await asyncio.gather(
            task_process_queue, task_update_xml, return_exceptions=True
        )
        #task_update_xml.cancel()
        #await asyncio.gather(task_process_queue, task_update_xml, return_exceptions=True)
        await asyncio.gather(task_process_queue, return_exceptions=True)
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
