# ========================================================================
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
# ========================================================================

import json
import os


class ConfigStore:
    MTConnectServerIP = None
    MTConnectServerPort = 0
    MQTTServerIP = None
    MQTTPort = 0
    Mapping_file = None
    Mapping_sheet = None
    Information_model = None
    DevicestreamName = None
    MTConnectNamespace = None
    Gateway_topic_prefix = None

    @staticmethod
    def load_config_json(vendor: str):
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

        ConfigStore.MTConnectServerIP = value["MTConnectServerIP"]
        ConfigStore.MTConnectServerPort = int(value["MTConnectServerPort"])
        ConfigStore.MQTTServerIP = value["MQTTServerIP"]
        ConfigStore.MQTTPort = int(value["MQTTPort"])
        ConfigStore.Mapping_file = value["Mapping_file"]
        ConfigStore.Mapping_sheet = value["Mapping_sheet"]
        ConfigStore.Information_model = value["Information_model"]
        ConfigStore.DevicestreamName = value["DevicestreamName"]
        ConfigStore.MTConnectNamespace = value["MTConnectNamespace"]
        ConfigStore.Gateway_topic_prefix = value["Gateway_topic_prefix"]
        print(f"[INFO] Configuration loaded for vendor '{vendor}'")
