# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

import asyncio

from services.data_conversion import try_convert_value


async def process_queue(data_queue, mapped_objects):
    """
    Process the data_queue by retrieving data from the queue and update the value of mapped objects.
    data_queue: The queue containing MQTT messages.
    mapped_objects: A list of MappedObject instances that hold data paths and values.
    """
    try:
        while True:
            mqtt_data = data_queue.get()
            for mapped_object in mapped_objects:
                mapped_object.value = get_value_from_json(
                    mqtt_data, mapped_object.opc_path
                )
                mapped_object.value = try_convert_value(
                    mapped_object.value, mapped_object.mtc_name
                )
            await asyncio.sleep(1)  # avoid tight loop
    except asyncio.CancelledError or KeyboardInterrupt:
        print("Processing queue task cancelled.")
        raise


def get_value_from_json(json_obj, path):
    """
    Retrieves a value from a nested JSON object using a slash-separated path.
    json_obj: The JSON object to search.
    path: A string representing the path to the desired value, using slashes (/) to separate keys.
    Returns the value if found, or None if not found or an error occurs.
    """
    keys = path.split("/")
    current = json_obj
    try:
        for key in keys:
            if isinstance(current, dict):
                if key in current:
                    current = current[key]
                else:
                    found = False
                    for sub_key in current:
                        if sub_key.startswith("<") and sub_key.endswith(">"):
                            current = current[sub_key]
                            if key in current:
                                current = current[key]
                                found = True
                                break
                    if not found:
                        return None
            else:
                return None
        if isinstance(current, dict) and "value" in current:
            return current["value"]
        return current
    except Exception:
        return None
