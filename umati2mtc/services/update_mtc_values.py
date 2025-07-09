# ========================================================================
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
# ========================================================================

#import xml.etree.ElementTree as ET
import asyncio

# Helper for Manipulation of XML state
from helper.xml_state import XmlState
from services.add_mtc_structure import add_element_to_stream, add_component_stream, find_value_in_stream

# Exceptions for error handling
from services.add_mtc_structure import MissingComponentStreamError, MissingElementError, DeviceStreamError

async def update_xml_with_values(mapped_objects, xml_state: XmlState, devicestream_name: str, mtc_ns: str):
    """
    Update the XML state with values from mapped_objects.
    This function runs in a separate asyncio task and continuously updates the XML state.
    mapped_objects: List of objects containing mtc_path, mtc_subtype and value.
    xml_state: XmlState instance containing the current XML state.
    mtc_ns: Namespace for MTConnect XML elements.
    """
    try:
        while True:
            for mapped_object in mapped_objects:
                print("__________________________________________________________________________________________________________________________")
                print(f"[UPDATING] Trying to update {mapped_object.mtc_path} (optional SubType {mapped_object.mtc_subtype}) with value: {mapped_object.value}")
                
                if mapped_object.value is not None:
                    component = mapped_object.mtc_path.split("/")[0]
                    component_name = mapped_object.mtc_path.split("/")[1]
                    element_name = mapped_object.mtc_path.split("/")[2]
                    new_value = str(mapped_object.value) # Call 
                    subtype = mapped_object.mtc_subtype

                    try:
                        find_value_in_stream(
                            xml_object=xml_state,
                            component=component,
                            component_name=component_name,
                            element_name=element_name,
                            new_value=new_value,
                            subtype=subtype,
                            ns = {'m': mtc_ns}  # Namespace mapping for MTConnect
                        )

                    except MissingComponentStreamError:
                        try:
                            add_component_stream(
                                xml_object=xml_state,
                                devicestream_name=devicestream_name,
                                component=component,
                                component_name=component_name,
                                ns = {'m': mtc_ns}  # Namespace mapping for MTConnect
                            )
                            add_element_to_stream(
                                xml_object=xml_state,
                                component=component,
                                component_name=component_name,
                                element_name=element_name,
                                value=new_value,
                                subtype=subtype,
                                ns = {'m': mtc_ns}  # Namespace mapping for MTConnect
                            )
                        except DeviceStreamError as e:
                            print(f"\033[91m[ERROR] Error adding ComponentStream for {component}/{component_name}: {e}\033[0m")
                            continue
                    except MissingElementError:
                        try:
                            add_element_to_stream(
                                xml_object=xml_state,
                                component=component,
                                component_name=component_name,
                                element_name=element_name,
                                value=new_value,
                                subtype=subtype,
                                ns = {'m': mtc_ns}  # Namespace mapping for MTConnect
                            )
                        except Exception as e:
                            print(f"\033[91m[ERROR] Error adding element to stream: {e}\033[0m")
                            continue
                    
            await asyncio.sleep(1)

    except asyncio.CancelledError:
        print("Update XML task cancelled.")
        raise
