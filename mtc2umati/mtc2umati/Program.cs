/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/

using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Opc.Ua;

namespace umatiConnect
{
    class Program
    {
        private static UmatiServer? _server;
        private static List<MappedObject> _mappedObjects = [];

        static async Task Main(string[] args)
        {
            try
            {
                // load the application configuration.
                ApplicationConfiguration config = await LoadApplicationConfiguration("umatiConnect.Server").ConfigureAwait(false);

                // auto accept any untrusted certificates.
                config.SecurityConfiguration.AutoAcceptUntrustedCertificates = true;

                // check the application certificate.
                await CheckApplicationInstanceCertificate(config).ConfigureAwait(false);

                // Start both the XML fetch and server in parallel
                Task fetchMTCXmlTask = FetchMTCXML();
                Task startServerTask = StartServer(config);
                Task umatiWriteValuesTask = UmatiWriteValues();

                // Wait for both tasks to complete (Note: The server will run indefinitely until the application is terminated (e.g., Ctrl-C))
                await Task.WhenAll(fetchMTCXmlTask, startServerTask);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task<ApplicationConfiguration> LoadApplicationConfiguration(string filePath)
        {
            try
            {
                ApplicationConfiguration config = await ApplicationConfiguration.Load(
                    filePath, ApplicationType.Server).ConfigureAwait(false);

                // ensure the application certificate is in the trusted peer store, else use dummy certificate
                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
                }
                return config;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading application configuration: {e.Message}");
                throw;
            }
        }

        private static async Task CheckApplicationInstanceCertificate(ApplicationConfiguration config)
        {
            try
            {
                // check the application instance certificate.
                CertificateIdentifier id = config.SecurityConfiguration.ApplicationCertificate;

                X509Certificate2 certificate = await id.Find(true).ConfigureAwait(false);

                if (certificate == null)
                {
                    throw new Exception("Application instance certificate not found: " + id);
                }

                Console.WriteLine("Application instance certificate found: {0}", id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error checking application instance certificate: {e.Message}");
                throw;
            }
        }

        private static async Task StartServer(ApplicationConfiguration config)
        {
            try
            {
                // Store the server instance at class level
                _server = new UmatiServer();
                _server.Start(config);

                Console.WriteLine("Server started: {0} at {1}", config.ApplicationName, config.ServerConfiguration.BaseAddresses[0]);
                Console.WriteLine("Press Ctrl-C to exit...");

                await Task.Delay(-1).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error starting server: {e.Message}");
                throw;
            }
        }

        private static void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            try
            {
                // allow
                e.Accept = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error validating certificate: {exception.Message}");
            }
        }

        #region Fetch MTC data
        private static async Task FetchMTCXML()
        {
            // Load the vendor configuration from JSON file
            string vendor = "dmg";
            VendorConfig vendorConfig = Load_configJSON(vendor);

            // Define the columns needed for mapping
            var columnsToRead = new[] {"OPC Path", "Data Type", "MTC Path" , "subType", "MTC Data Type"};
            var columnsToIgnore = new[] {"subType"};

            _mappedObjects = MappingLoader.LoadMapping(vendorConfig.Mapping_file!, vendorConfig.Mapping_sheet!, columnsToRead, columnsToIgnore)  ?? [];;

            // Validate vendor config for MTC connection
            string url = vendorConfig.MTCServerIP ?? throw new ArgumentNullException(nameof(vendorConfig.MTCServerIP), "MTCServerIP cannot be null.");;
            int port = vendorConfig.MTCServerPort;
            string mtcNamespace = vendorConfig.MTCNamespace ?? throw new ArgumentNullException(nameof(vendorConfig.MTCNamespace), "MTCNamespace cannot be null.");;
            int intervalMilliseconds = 1000;

            Console.WriteLine($"Starting XML fetch loop with URL: {url} and Port: {port}");

            // Run the fetch loop
            await XmlFetchLoopRunner.RunXmlFetchLoopAsync(url, port, mtcNamespace, _mappedObjects, intervalMilliseconds);
        }

        private static async Task UmatiWriteValues()
        {
            if (_server == null || _mappedObjects.Count == 0)
            {
                Console.WriteLine("Server is not initialized or no mapped objects available. Have you closed the mapping.xlsx file?");
                return;
            }

            var writer = new UmatiWriter(_server);
            while (true)
            {
                await writer.UpdateNodesAsync(_mappedObjects, "DMGMilltap700").ConfigureAwait(false);
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
        #endregion


        #region Load config.json
        public static VendorConfig Load_configJSON(string vendor)
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

            return new VendorConfig
            {
                MTCServerIP = config[vendor]["MTConnectServerIP"],
                MTCServerPort = int.Parse(config[vendor]["MTConnectServerPort"]),
                MTCNamespace = config[vendor]["MTConnectNamespace"],
                Mapping_file = config[vendor]["Mapping_file"],
                Mapping_sheet = config[vendor]["Mapping_sheet"],
                Mode = int.Parse(config[vendor]["Mode"])
            };
        }
        public class VendorConfig
        {
            public string? MTCServerIP { get; set; }
            public int MTCServerPort { get; set; }
            public string? MTCNamespace { get; set; }
            public string? Mapping_file { get; set; }
            public string? Mapping_sheet { get; set; }
            public int Mode { get; set; }
        }
        #endregion
    }
}

