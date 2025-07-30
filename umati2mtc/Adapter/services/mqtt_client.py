# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover. All rights reserved.

import json
import asyncio
import paho.mqtt.client as mqtt
import time

# MQTT callback: message received
def on_message(client, userdata, msg):
    global mqtt_data
    try:
        payload = json.loads(msg.payload.decode())
        userdata.put(payload)  # Put message in the shared queue
        # Save to file with timestamp
        # with open("mqtt_messages.jsonl", "a") as f:
        #     json.dump({"topic": msg.topic, "payload": payload}, f)
        #     f.write("\n")
        #     time.sleep(5)
    except Exception as e:
        print("Error processing MQTT message:", e)

# Start MQTT client in separate thread
def start_mqtt(IP, port, topic_prefix, message_queue):
    try:
        client = mqtt.Client(userdata=message_queue)
        client.on_message = on_message
        client.connect(IP, port, 60)
        client.subscribe(topic_prefix)
        client.loop_start()  # run MQTT in background thread
        print(
            f"\033[92m[INFO] MQTT client started and connected to {IP}:{port} -> subscribed to {topic_prefix}\033[0m"
        )
    except Exception as e:
        print(f"\033[91m[ERROR] Failed to start MQTT client: {e}\033[0m")
        print("Retrying in 10 seconds...")
        asyncio.get_event_loop().call_later(10, start_mqtt, IP, port, topic_prefix, message_queue)