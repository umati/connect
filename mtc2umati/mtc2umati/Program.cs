/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/

global using System;
global using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Opc.Ua;
using mtc2umati.Services;

namespace mtc2umati
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

                // load the vendor configuration
                ConfigStore.LoadConfigJSON("mazak");

                // Start both the XML fetch and server in parallel
                Task startServerTask = StartServer(config);
                Task fetchMTCXmlTask = FetchMTCXML();
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

                X509Certificate2 certificate = await id.Find(true).ConfigureAwait(false) ?? throw new Exception("Application instance certificate not found: " + id);
                Console.WriteLine("Application instance certificate found: {0}", id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error checking application instance certificate: {e.Message}");
                throw;
            }
        }
        #region Start OPC UA Server
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
        #endregion
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
            // Load the mapping from the Excel file
            Console.WriteLine("Creating the mapping between MTC and OPC UA...");
            _mappedObjects = MappingLoader.LoadMapping(ConfigStore.VendorSettings.Mapping_file!, ConfigStore.VendorSettings.Mapping_sheet!) ?? []; ;

            // Validate vendor config for MTC connection
            string url = ConfigStore.VendorSettings.MTCServerIP ?? throw new ArgumentNullException(nameof(ConfigStore.VendorSettings.MTCServerIP), "MTCServerIP cannot be null."); ;
            int port = ConfigStore.VendorSettings.MTCServerPort;
            string mtcNamespace = ConfigStore.VendorSettings.MTCNamespace ?? throw new ArgumentNullException(nameof(ConfigStore.VendorSettings.MTCNamespace), "MTCNamespace cannot be null."); ;

            // Run the fetch loop
            Console.WriteLine($"Starting MTConnect XML fetch loop with URL: {url} and Port: {port}");
            await XmlFetchLoopRunner.RunXmlFetchLoopAsync(url, port, mtcNamespace, _mappedObjects);
        }
        #endregion

        #region Write OPC UA values
        private static async Task UmatiWriteValues()
        {
            if (_server == null || _mappedObjects.Count == 0)
            {
                Console.WriteLine("Server is not initialized or no mapped objects available. Have you closed the mapping.xlsx file?");
                return;
            }
            var writer = new UmatiWriter(_server);
            await writer.UpdateNodesAsync(_mappedObjects).ConfigureAwait(false);
        }
        #endregion
    }
}

