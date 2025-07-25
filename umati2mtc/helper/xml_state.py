# ========================================================================
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
# ========================================================================

import xml.etree.ElementTree as ET
from threading import Lock


class XmlState:
    """
    Thread-safe wrapper for a live XML ElementTree.
    """

    def __init__(self, tree: ET.ElementTree):
        self._tree = tree
        self._lock = Lock()

    def get_tree(self) -> ET.ElementTree:
        """
        Thread-safe access to current XML tree.
        """
        with self._lock:
            return self._tree

    def update_tree(self, new_tree: ET.ElementTree):
        """
        Thread-safe replacement of the XML tree.
        """
        with self._lock:
            self._tree = new_tree

    def get_root(self) -> ET.Element:
        """
        Direct access to the root element (used by xml_manipulator).
        """
        with self._lock:
            return self._tree.getroot()

    def to_string(self) -> str:
        """
        Convert current XML to a string.
        """
        with self._lock:
            return ET.tostring(
                self._tree.getroot(), encoding="utf-8", xml_declaration=True
            ).decode("utf-8")


def initialize_xml_state(xml_file_path: str) -> XmlState:
    try:
        tree = ET.parse(xml_file_path)
        return XmlState(tree)
    except Exception as e:
        print(f"[ERROR] Error loading XML file '{xml_file_path}': {e}")
        raise

def update_creation_time(xml_state, mtc_ns: str):
    """
    Update the creationTime attribute in the MTConnectStreams Header.
    
    Args:
        xml_state (XmlState): The current XML state object.
        mtc_ns (str): The MTConnect namespace, e.g., "urn:mtconnect.org:MTConnectStreams:1.3"
    """
    ns_map = {'m': mtc_ns}
    root = xml_state.get_root()
    header = root.find('m:Header', ns_map)
    if header is not None:
        new_time = datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%SZ")
        header.set('creationTime', new_time)
        #print(f"[INFO] Updated creationTime to {new_time}")
    else:
        print("\033[91m[ERROR] Header element not found in XML.\033[0m")