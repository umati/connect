# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

"""
Data type conversion utilities for OPC UA to MTConnect mapping.

This module handles conversion of OPC UA data types to MTConnect-compatible formats.
"""

from typing import Any, Optional


def _convert_execution_value(value: str) -> str:
    """Convert execution status values."""
    match value:
        case "Initializing":
            return "READY"
        case "Running":
            return "ACTIVE"
        case "Ended":
            return "STOPPED"
        case _:
            return "UNAVAILABLE"


def _convert_light_state(value: str) -> str:
    """Convert light state values."""
    match value:
        case "1":
            return "ON"
        case "0":
            return "OFF"
        case _:
            return "UNAVAILABLE"


def _convert_mode_value(value: str, mode_type: str) -> str:
    """Convert mode values (Controller/Operation)."""
    if mode_type == "ControllerMode":
        match value:
            case "1":
                return "MANUAL"
            case "0":
                return "AUTOMATIC"
            case _:
                return "UNAVAILABLE"
    else:  # OperationMode
        match value:
            case "1":
                return "AUTOMATIC"
            case "0":
                return "MANUAL"
            case _:
                return "UNAVAILABLE"


def _handle_special_conversions(
    value: Any, mtc_name: str
) -> Optional[str | int | float]:
    """Handle special conversion cases that need early returns."""
    # Gateway throws an exception "Opc.Ua..." when nodes are missing
    if "Opc.Ua" in str(value):
        return "UNAVAILABLE"

    # Handle LocalizedText (dict type)
    if isinstance(value, dict):
        text_value = value.get("text")
        if mtc_name == "Execution":
            return _convert_execution_value(str(text_value))
        return text_value

    # Handle PowerOnTime conversion
    if mtc_name == "PowerOnTime":
        try:
            return int(value) / 1000
        except ValueError:
            return None

    return None


def convert_value(value: Any, mtc_name: str) -> Optional[str | int | float]:
    """Convert OPC UA value to MTConnect data type based on variable name."""
    if value is None:
        return None

    # Check for special conversions that need early returns
    special_result = _handle_special_conversions(value, mtc_name)
    if special_result is not None:
        return special_result

    # Handle remaining conversions
    result: str | int | float | None = None

    if isinstance(value, float):  # Handle Range
        try:
            result = str(int(value))
        except ValueError:
            result = value
    elif mtc_name.startswith("LightState"):
        result = _convert_light_state(str(value))
    elif "Override" in mtc_name:
        try:
            result = int(value)
        except ValueError:
            result = str(value)
    elif mtc_name in ("ControllerMode", "OperationMode"):
        result = _convert_mode_value(str(value), mtc_name)
    else:
        # Handle other data types as needed
        result = str(value)

    return result


def try_convert_value(value: Any, mtc_name: str) -> Optional[str | int | float]:
    """Safely convert value to MTConnect format with error handling."""
    try:
        return convert_value(value, mtc_name)
    except (ValueError, TypeError, AttributeError) as e:
        print(f"[ERROR] Conversion failed for {mtc_name}: {e}")
        return None
