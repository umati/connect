# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover. All rights reserved.

import json
import time
import random
import paho.mqtt.client as mqtt
import os

BROKER_IP = os.getenv("MQTT_BROKER_IP")
BROKER_PORT = os.getenv("MQTT_BROKER_PORT")
BROKER_PORT = int(os.getenv("MQTT_BROKER_PORT"))
INPUT_FILE = "mqtt_messages.jsonl"
PUBLISH_INTERVAL = 1.0         # Seconds between messages

# Load the first message line
with open(INPUT_FILE, "r") as f:
    first_line = f.readline()
    if not first_line:
        raise RuntimeError("No lines found in MQTT message file.")
    msg = json.loads(first_line)

topic = msg["topic"]
base_payload = msg["payload"]

# Create MQTT client
client = mqtt.Client()
client.connect(BROKER_IP, BROKER_PORT, 60)
client.loop_start()

print(f"Simulating messages on topic '{topic}'...")

try:
    while True:
        # Modify FeedOverride with a random value
        payload = json.loads(json.dumps(base_payload))  # Deep copy
        new_value = random.randint(2, 104)
        try:
            payload["Monitoring"]["MachineTool"]["FeedOverride"]["value"] = new_value
        except KeyError as e:
            print(f"[ERROR] Could not update FeedOverride: {e}")
            break

        payload_json = json.dumps(payload)
        client.publish(topic, payload_json)
        print(f"Published to {topic} | FeedOverride: {new_value}")
        time.sleep(PUBLISH_INTERVAL)
except KeyboardInterrupt:
    print("Simulation stopped.")
finally:
    client.loop_stop()
    client.disconnect()
