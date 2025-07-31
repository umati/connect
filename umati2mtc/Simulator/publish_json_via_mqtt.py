# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover. All rights reserved.

import json
import os
import random
import time

import paho.mqtt.client as mqtt

BROKER_IP = os.getenv("MQTT_BROKER_IP", "localhost")
BROKER_PORT = int(os.getenv("MQTT_BROKER_PORT", 1883))
INPUT_FILE = "mqtt_message.json"
PUBLISH_INTERVAL = 1  # Seconds between messages

# Load the full JSON object
with open(INPUT_FILE, "r", encoding="utf-8") as f:
    msg = json.load(f)

# Extract fields
topic = msg["topic"]
base_payload = msg["payload"]

# Create MQTT client
client = mqtt.Client()
client.connect(BROKER_IP, BROKER_PORT, 60)
client.loop_start()

print(f"Simulating messages on topic '{topic}'...")

try:
    power_on_time = 0
    while True:
        # Simulate a random value for FeedOverride
        payload = json.loads(json.dumps(base_payload))
        feed_override = random.randint(0, 120)
        try:
            payload["Monitoring"]["MachineTool"]["FeedOverride"]["value"] = (
                feed_override
            )
            payload["Monitoring"]["MachineTool"]["PowerOnDuration"] = power_on_time
        except KeyError as e:
            print(f"[ERROR] Could not update FeedOverride: {e}")
            break

        payload_json = json.dumps(payload)
        client.publish(topic, payload_json)
        print(
            f"Published to {topic} | FeedOverride: {feed_override} | PowerOnDuration: {power_on_time}"
        )
        power_on_time += PUBLISH_INTERVAL
        time.sleep(PUBLISH_INTERVAL)
except KeyboardInterrupt:
    print("Simulation stopped.")
finally:
    client.loop_stop()
    client.disconnect()
