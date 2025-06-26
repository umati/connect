# ========================================================================
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
# ========================================================================

import pandas as pd

class MappedObject:
    """
    Stores OPC UA to MTConnect mapping data including value.
    """
    def __init__(self, opc_path: str, opc_datatype: str, mtc_name: str, mtc_path: str, mtc_subtype: str, mtc_datatype: str, value):
        self.opc_path = opc_path
        self.opc_datatype = opc_datatype
        self.mtc_name = mtc_name
        self.mtc_path = mtc_path
        self.mtc_subtype = mtc_subtype
        self.mtc_datatype = mtc_datatype
        self.value = value

def load_mapping(mapping_file_path, sheet_name):
    """
    Reads the Excel mapping file and returns a list of MappedObject instances.

    Skips:
    - Rows where "MTC Path" is None
    - Rows where "MTC Path" starts with "#"
    - Rows containing braces "{}" or angle brackets "<>"
    - If "subType" is None, it is set to the string "None"
    """
    df_mapping = pd.read_excel(mapping_file_path, sheet_name, header=0)
    mapped_objects = []

    for _, row in df_mapping.iterrows():
        mtc_path = str(row["MTC Path"]).strip() if pd.notna(row["MTC Path"]) else None

        if (
            mtc_path is None or
            mtc_path.startswith("#") or
            "{" in mtc_path or
            "}" in mtc_path or
            "<" in mtc_path or
            ">" in mtc_path
        ):
            continue

        sub_type = row["subType"] if pd.notna(row["subType"]) else "None"

        mapped_object = MappedObject(
            opc_path=row["OPC Path"],
            opc_datatype=row["Data Type"],
            mtc_name=row["MTC Name"],
            mtc_path=mtc_path,
            mtc_subtype=sub_type,
            mtc_datatype=row["MTC Data Type"],
            value=None
        )
        mapped_objects.append(mapped_object)

    return mapped_objects

