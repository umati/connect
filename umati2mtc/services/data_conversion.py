# ========================================================================
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
# ========================================================================


# general conversion function
def convert_value(value, mtc_name):
    """
    Convert the value to the specified MTConnect data type.
    :param value: The value to convert.
    :param mtc_name: The name of the MTConnect data type.
    :return: The converted value.
    """
    if value is None:
        return None

    elif "Opc.Ua" in str(value):
        value = "UNAVAILABLE"  # Replace with actual condition for unavailable value
        return value
        
    else:
        if type(value) is dict:  # Handle LocalizedText
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

        elif type(value) is float:  # Handle Range
            try:
                value = int(value)  # Convert to integer
                return str(value)
            except ValueError:
                return value

        elif mtc_name.startswith("LightState"):
            match str(value):
                case "1":
                    value = "ON"
                case "0":
                    value = "OFF"
                case _:
                    value = "UNAVAILABLE"
            return value

        elif "Override" in mtc_name:
            try:
                value = int(value)  # Convert to integer
                return str(value)
            except ValueError:
                return value

        elif mtc_name == "PowerOnTime":
            try:
                value = int(value) / 1000  # Convert milliseconds to seconds
                return str(value)
            except ValueError:
                return None

        elif mtc_name == "ControllerMode":
            match str(value):
                case "1":
                    value = "MANUAL"
                case "0":
                    value = "AUTOMATIC"
                case _:
                    value = "UNAVAILABLE"
            return value

        elif mtc_name == "OperationMode":
            match str(value):
                case "1":
                    value = "AUTOMATIC"
                case "0":
                    value = "MANUAL"
                case _:
                    value = "UNAVAILABLE"

            return value

        else:
            # Handle other data types as needed
            return str(value)


# ========================================================================#

# Helper for conversion of values from OPC UA to MTConnect data types


def try_convert_value(value, mtc_name):
    """
    Attempt to convert the value to the specified MTConnect data type.
    :param value: The value to convert.
    :param mtc_name: The name of the MTConnect data type.
    :return: The converted value or None if conversion fails.
    """
    try:
        return convert_value(value, mtc_name)
    except Exception as e:
        print(f"[ERROR] Conversion failed for {mtc_name}: {e}")
        return None
