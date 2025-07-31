# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

"""
Configuration management for the UMATI to MTConnect adapter.
This module handles loading and storing configuration settings from JSON files.
"""

import json
import os


class ConfigStore:
    """Static configuration store for mapping file settings."""

    Mapping_file = None
    Mapping_sheet = None

    @staticmethod
    def load_config_json(vendor: str):
        """Load vendor-specific configuration from JSON file."""
        config_path = "./config.json"
        if not os.path.exists(config_path):
            raise FileNotFoundError(
                f"[ERROR] Configuration file not found at {config_path}"
            )

        with open(config_path, "r", encoding="utf-8") as file:
            config = json.load(file)

        if vendor not in config:
            raise KeyError(f"[ERROR] Vendor '{vendor}' not found in config.")

        value = config[vendor]

        ConfigStore.Mapping_file = value["Mapping_file"]
        ConfigStore.Mapping_sheet = value["Mapping_sheet"]

    @staticmethod
    def print_config(vendor: str):
        """Print current configuration values."""
        print(f"[INFO] Configuration loaded for vendor '{vendor}'")
