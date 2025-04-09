using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


namespace mtc2umati
{
    public static class ConfigStore
    {
        public static VendorConfig VendorSettings { get; set; } = new VendorConfig();

        public static void LoadConfigJSON(string vendor)
        {
            string configPath = "./config.json";
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Configuration file not found at {configPath}");
            }

            string json = File.ReadAllText(configPath);

            var config = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

            if (config == null || !config.ContainsKey(vendor))
            {
                throw new KeyNotFoundException($"Vendor '{vendor}' not found in config.");
            }

            VendorSettings = new VendorConfig
            {
                MTCServerIP = config[vendor]["MTConnectServerIP"],
                MTCServerPort = int.Parse(config[vendor]["MTConnectServerPort"]),
                MTCNamespace = config[vendor]["MTConnectNamespace"],
                Mapping_file = config[vendor]["Mapping_file"],
                Mapping_sheet = config[vendor]["Mapping_sheet"],
                Information_model = config[vendor]["Information_model"],
                OPCNamespace = config[vendor]["OPCNamespace"],
                Machine_Name = config[vendor]["Machine_Name"],
                Mode = int.Parse(config[vendor]["Mode"])
            };
        }
    }

    public class VendorConfig
    {
        public string? MTCServerIP { get; set; }
        public int MTCServerPort { get; set; }
        public string? MTCNamespace { get; set; }
        public string? Mapping_file { get; set; }
        public string? Mapping_sheet { get; set; }
        public string? Information_model { get; set; }
        public string? OPCNamespace { get; set; }
        public string? Machine_Name { get; set; }
        public int Mode { get; set; }
    }
}
