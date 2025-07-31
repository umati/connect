# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

"""
Data type conversion utilities for OPC UA to MTConnect mapping.

This module handles conversion of OPC UA data types to MTConnect-compatible formats.
"""

from typing import Any, Optional


def convert_value(value: Any, mtc_name: str) -> Optional[str]:
    """Convert OPC UA value to MTConnect data type based on variable name."""
    if value is None:
        return None

    # Gateway throws an exception "Opc.Ua..." when nodes are missing
    if "Opc.Ua" in str(value):
        return "UNAVAILABLE"

    if isinstance(value, dict):  # Handle LocalizedText
        value = value.get("text")
        if mtc_name == "Execution":
            match str(value):
                case "Initializing":
                    value = "READY"
                case "Running":
                    value = "ACTIVE"
                case "Ended":
                    value = "STOPPED"
                case _:
                    value = "UNAVAILABLE"
        return value

    if isinstance(value, float):  # Handle Range
        try:
            value = int(value)  # Convert to integer
            return str(value)
        except ValueError:
            return value

    if mtc_name.startswith("LightState"):
        match str(value):
            case "1":
                value = "ON"
            case "0":
                value = "OFF"
            case _:
                value = "UNAVAILABLE"
        return value

    if "Override" in mtc_name:
        try:
            value = int(value)  # Convert to integer
            return str(value)
        except ValueError:
            return value

    if mtc_name == "PowerOnTime":
        try:
            value = int(value) / 1000  # Convert milliseconds to seconds
            return str(value)
        except ValueError:
            return None

    if mtc_name == "ControllerMode":
        match str(value):
            case "1":
                value = "MANUAL"
            case "0":
                value = "AUTOMATIC"
            case _:
                value = "UNAVAILABLE"
        return value

    if mtc_name == "OperationMode":
        match str(value):
            case "1":
                value = "AUTOMATIC"
            case "0":
                value = "MANUAL"
            case _:
                value = "UNAVAILABLE"
        return value

    # Handle other data types as needed
    return str(value)


def try_convert_value(value: Any, mtc_name: str) -> Optional[str]:
    """Safely convert value to MTConnect format with error handling."""
    try:
        return convert_value(value, mtc_name)
    except (ValueError, TypeError, AttributeError) as e:
        print(f"[ERROR] Conversion failed for {mtc_name}: {e}")
        return None
