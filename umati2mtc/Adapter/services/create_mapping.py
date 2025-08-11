# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

"""
Mapping creation utilities for OPC UA to MTConnect data mapping.

This module handles loading and processing Excel mapping files.
"""

from dataclasses import dataclass
from typing import Optional

import pandas as pd  # type: ignore[import-untyped]


@dataclass
class MappedObject:
    """Data structure for OPC UA to MTConnect mapping with current value."""

    opc_path: str
    opc_datatype: str
    mtc_name: str
    mtc_path: str
    mtc_specname: Optional[str]
    mtc_datatype: str
    value: Optional[str] = None

    def get_mtc_path(self) -> str:
        """Get the MTConnect path."""
        return self.mtc_path

    def set_value(self, value: Optional[str]) -> None:
        """Set the current value."""
        self.value = value


def load_mapping(mapping_file_path: str, sheet_name: str) -> list[MappedObject]:
    """Load mapping data from Excel file and create MappedObject instances."""
    df_mapping = pd.read_excel(mapping_file_path, sheet_name, header=0)
    mapped_objects = []

    for _, row in df_mapping.iterrows():
        mtc_path = str(row["MTC Path"]).strip() if pd.notna(row["MTC Path"]) else None

        if (
            mtc_path is None
            or "{" in str(mtc_path)
            or "}" in str(mtc_path)
            or "<" in str(mtc_path)
            or ">" in str(mtc_path)
        ):
            continue

        mapped_object = MappedObject(
            opc_path=row["OPC Path"],
            opc_datatype=row["Data Type"],
            mtc_name=row["MTC Name"],
            mtc_path=mtc_path,
            mtc_specname=row["SpecName"] if pd.notna(row["SpecName"]) else None,
            mtc_datatype=row["MTC Data Type"],
            value=None,
        )
        mapped_objects.append(mapped_object)

    return mapped_objects
