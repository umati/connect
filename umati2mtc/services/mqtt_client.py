# ========================================================================
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
# ========================================================================

import paho.mqtt.client as mqtt
import json
   
# MQTT callback: message received
def on_message(client, userdata, msg):
    global mqtt_data
    try:
        payload = json.loads(msg.payload.decode())
        userdata.put(payload)  # Put message in the shared queue
        #print(f"\033[92m[MQTT] Received message on topic {msg.topic}: {payload}\033[0m")
    except Exception as e:
        print("Error processing MQTT message:", e)

# Start MQTT client in separate thread
def start_mqtt(IP, port, topic_prefix, message_queue):
    client = mqtt.Client(userdata=message_queue)
    client.on_message = on_message
    client.connect(IP, port, 60)
    client.subscribe(topic_prefix)
    client.loop_start()  # run MQTT in background thread
    print(f"\033[92m[MQTT] MQTT client started and connected to {IP}:{port} -> subscribed to {topic_prefix}\033[0m")
