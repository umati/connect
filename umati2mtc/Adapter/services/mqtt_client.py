# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

"""
MQTT client service for receiving OPC UA umati data.

This module handles MQTT connection, subscription, and message processing.
"""

import asyncio
import json

try:
    import paho.mqtt.client as mqtt  # type: ignore[import-untyped]
except ImportError:
    print("[ERROR] paho-mqtt not installed. => pip install paho-mqtt")
    raise


def on_message(_client, userdata, msg):
    """Processes received MQTT messages and adds to queue."""
    try:
        payload = json.loads(msg.payload.decode())
        userdata.put(payload)  # Put message in the shared queue
    except (json.JSONDecodeError, UnicodeDecodeError) as e:
        print(f"Error processing MQTT message: {e}")


def start_mqtt(broker_ip: str, port: int, topic_prefix: str, message_queue):
    """Start MQTT client and subscribe to specified topics."""
    try:
        client = mqtt.Client(userdata=message_queue)
        client.on_message = on_message
        client.connect(broker_ip, port, 60)
        client.subscribe(topic_prefix)
        client.loop_start()  # run MQTT in background thread
        print(
            f"[INFO] MQTT client connected to {broker_ip}:{port} -> subscribed to {topic_prefix}"
        )

    except (ConnectionRefusedError, OSError, ValueError) as e:
        print(f"[ERROR] Failed to start MQTT client: {e}")
        print("Retrying in 10 seconds...")
        asyncio.get_event_loop().call_later(
            10, start_mqtt, broker_ip, port, topic_prefix, message_queue
        )
