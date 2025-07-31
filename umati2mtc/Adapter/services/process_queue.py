# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

"""
Message queue processing service.

This module handles processing MQTT messages from the queue and updating mapped objects.
"""

import asyncio

from .data_conversion import try_convert_value


async def process_queue(data_queue, mapped_objects):
    """Process MQTT messages from queue and update mapped objects with values."""
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
    except asyncio.CancelledError:
        print("Processing queue task cancelled.")
        raise
    except KeyboardInterrupt:
        print("Processing queue task cancelled.")
        raise


def get_value_from_json(json_obj, path):
    """Extract value from nested JSON using slash-separated path."""
    keys = path.split("/")
    current = json_obj
    try:
        for key in keys:
            if not isinstance(current, dict):
                return None

            if key in current:
                current = current[key]
                continue

            # Handle special keys with angle brackets
            for sub_key in current:
                if sub_key.startswith("<") and sub_key.endswith(">"):
                    current = current[sub_key]
                    if key in current:
                        current = current[key]
                        break
                    return None
            return None

        if isinstance(current, dict) and "value" in current:
            return current["value"]
        return current
    except (KeyError, TypeError, AttributeError):
        return None
