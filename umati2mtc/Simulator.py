import json
import time
import paho.mqtt.client as mqtt

BROKER_IP = "localhost"  # or your real broker IP
BROKER_PORT = 1883
INPUT_FILE = "mqtt_messages.jsonl"
PUBLISH_INTERVAL = 1.0  # seconds between messages

client = mqtt.Client()
client.connect(BROKER_IP, BROKER_PORT, 60)
client.loop_start()

with open(INPUT_FILE, "r") as f:
    for line in f:
        msg = json.loads(line)
        topic = msg["topic"]
        payload = json.dumps(msg["payload"])
        client.publish(topic, payload)
        print(f"Published to {topic}: {payload}")
        time.sleep(PUBLISH_INTERVAL)

client.loop_stop()
client.disconnect()
