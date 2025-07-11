# ========================================================================
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
# ========================================================================

import xml.etree.ElementTree as ET
from datetime import datetime, timezone

from helper.xml_state import XmlState

# Exceptions for error handling
class MissingComponentStreamError(Exception):
    pass

class MissingElementError(Exception):
    pass

class MissingSubcomponentError(Exception):
    pass

class DeviceStreamError(Exception):
    pass

def find_value_in_stream(xml_object: XmlState, component: str, component_name: str,
                    element_name: str, new_value: str, subtype: str = None, ns: dict = None) -> ET.ElementTree:
    """
    Find and update an element in the XML object at the correct ComponentStream location.
    xml_object: XmlState instance containing the current XML tree.
    component: The component type (e.g., "Controller", "Device").
    component_name: The name of the component instance.
    element_name: The name of the element to be updated.
    new_value: The new text value to assign to the element.
    subtype: Optional subType attribute for the element.
    ns: Namespace mapping for MTConnect (e.g., {'m': 'urn:mtconnect.org:MTConnectDevices:1.3'}).
    """
    root = xml_object.get_root()
    xpath = f".//m:ComponentStream[@component='{component}'][@name='{component_name}']"
    component_streams = root.findall(xpath, ns)

    if not component_streams:
        raise MissingComponentStreamError(f"No ComponentStream found with component='{component}' and name='{component_name}'.")
    
    #print(f"[UPDATING] Found {len(component_streams)} ComponentStream(s) for component='{component}' and name='{component_name}'.")
    current_time = datetime.now(timezone.utc).isoformat().replace('+00:00', 'Z')

    for cs in component_streams:
        for elem in cs.iterfind(f".//m:{element_name}", ns):
            if subtype == "None" or elem.attrib.get('subType') == subtype:
                elem.text = new_value
                elem.set("timestamp", current_time)
                #print(f"\033[92m[SUCCESS] Updated {element_name} with value '{new_value}'\033[0m")
                return
    raise MissingElementError(f"Element '{element_name}' with optional subType='{subtype}' not found.")


def add_element_to_stream(xml_object: XmlState, component: str, component_name: str,
                          element_name: str, value: str, subtype: str = None, ns: dict = None) -> ET.ElementTree:
    """
    Add a new element to the XML object at the correct ComponentStream location.
        xml_object: XmlState instance containing the current XML tree.
        component: The component type (e.g., "Controller", "Device").
        component_name: The name of the component instance.
        element_name: The name of the element to be added.
        value: The text value to assign to the new element.
        subtype: Optional subType attribute for the new element.
        ns: Namespace mapping for MTConnect (e.g., {'m': 'urn:mtconnect.org:MTConnectDevices:1.3'}).
    """
    root = xml_object.get_root()
    subcomponent = "Samples" # Has to be Samples, Events or Condition

    # Build the XPath to locate the desired ComponentStream
    xpath = f".//m:ComponentStream[@component='{component}'][@name='{component_name}']"
    
    # Obtain the current UTC timestamp in ISO 8601 format, ending with 'Z'
    current_time = datetime.now(timezone.utc).isoformat().replace('+00:00', 'Z')
    
    # Find the first matching ComponentStream
    component_stream = root.find(xpath, ns)
    if component_stream is None:
        raise MissingComponentStreamError(f"No ComponentStream found with component='{component}' and name='{component_name}'.")

    # Go through the ComponentStream to find the specified subcomponent
    subcomponent_element = component_stream.find(f".//m:{subcomponent}", ns)
    if subcomponent_element is None:
        raise MissingSubcomponentError(f"No subcomponent '{subcomponent}' found in ComponentStream for component='{component}' and name='{component_name}'.")

    # Create a namespaced tag for the new element: {namespace}ElementName
    # This ensures the element is placed in the same MTConnect namespace as its parent.
    namespace_uri = ns.get('m') if ns and 'm' in ns else None
    if namespace_uri:
        qualified_name = f"{{{namespace_uri}}}{element_name}"
    else:
        qualified_name = element_name  # Fallback if no namespace provided
    
    # Create the new element in the specified subcomponent
    new_element = ET.SubElement(subcomponent_element, qualified_name)
    new_element.text = value
    new_element.set('timestamp', current_time)
    
    # If a subType was provided, set it as an attribute
    if subtype:
        new_element.set('subType', subtype)
    
    # Log an informational message (output goes to stdout)
    print(f"\033[92m[SUCCESS] Added new element '{element_name}' with subType='{subtype}' and value='{value}' to ComponentStream.\033[0m")


def add_component_stream(xml_object: XmlState, devicestream_name: str, component: str, component_name: str, ns: dict) -> ET.ElementTree:
    root = xml_object.get_root()
    """
    Add a new ComponentStream to the specified DeviceStream in the XML object.
    xml_object: XmlState instance containing the current XML tree.
    devicestream_name: The name of the DeviceStream to which the ComponentStream will be added.
    component: The component type (e.g., "Controller", "Device").
    component_name: The name of the component instance.
    ns: Namespace mapping for MTConnect (e.g., {'m': 'urn:mtconnect.org:MTConnectDevices:1.3'}).
    """
    # Ensure that we have a valid namespace URI under the 'm' prefix
    namespace_uri = ns.get('m')
    if not namespace_uri:
        raise ValueError("Namespace mapping must include an 'm' key with a valid URI.")

    # Register the 'm' prefix with ElementTree so that output uses the 'm' prefix
    ET.register_namespace('m', namespace_uri)

    # Go through the XML tree to find the specified Devicestream
    device_stream = root.find(f".//m:DeviceStream[@name='{devicestream_name}']", ns)
    if device_stream is None:
        raise DeviceStreamError(f"No DeviceStream found with name='{devicestream_name}'.")

    # Build the namespace-qualified tag for the ComponentStream
    comp_stream_tag = f"{{{namespace_uri}}}ComponentStream"

    # Define the required subelements with their qualified names
    samples_tag = f"{{{namespace_uri}}}Samples"
    events_tag = f"{{{namespace_uri}}}Events"
    condition_tag = f"{{{namespace_uri}}}Condition"

    # Create the ComponentStream element with the component and name attributes
    new_component_stream = ET.Element(
        comp_stream_tag,
        {
            'component': component,
            'name': component_name
        }
    )

    # Append the three required subelements: Samples, Events, and Condition
    ET.SubElement(new_component_stream, samples_tag)
    ET.SubElement(new_component_stream, events_tag)
    ET.SubElement(new_component_stream, condition_tag)

    # Append the new ComponentStream to the devicestream element
    device_stream.append(new_component_stream)

    # Update the XmlState's internal tree with our modified tree
    xml_object.update_tree(ET.ElementTree(root))

    print(f"\033[92m[SUCCESS] Added new ComponentStream for component='{component}' and name='{component_name}'.\033[0m")