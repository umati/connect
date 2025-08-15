# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

"""
MQTT simulator for testing UMATI to MTConnect integration.

This module publishes simulated OPC UA umati JSON messages via MQTT broker.
"""

import json
import os
import random
import time

try:
    import paho.mqtt.client as mqtt
except ImportError:
    print("[ERROR] paho-mqtt not installed. => pip install paho-mqtt")
    raise


BROKER_IP = os.getenv("MQTT_BROKER_IP", "localhost")
BROKER_PORT = int(os.getenv("MQTT_BROKER_PORT", "1883"))
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


def main():
    """Main simulation loop that publishes MQTT messages with simulated data."""
    try:
        on_time = 0
        while True:
            # Simulate a random value for FeedOverride
            payload = json.loads(json.dumps(base_payload))
            feed_override = random.randint(0, 120)
            try:
                payload["Monitoring"]["MachineTool"]["FeedOverride"][
                    "value"
                ] = feed_override
                payload["Monitoring"]["MachineTool"]["PowerOnDuration"] = on_time
            except KeyError as e:
                print(f"[ERROR] Could not update FeedOverride: {e}")
                break

            payload_json = json.dumps(payload)
            client.publish(topic, payload_json)
            print(
                f"Published to {topic} | FeedOverride: {feed_override} | PowerOnDuration: {on_time}"
            )
            on_time += PUBLISH_INTERVAL
            time.sleep(PUBLISH_INTERVAL)
    except KeyboardInterrupt:
        print("Simulation stopped.")
    finally:
        client.loop_stop()
        client.disconnect()


if __name__ == "__main__":
    main()
