/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/
 
using System;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Opc.Ua;

namespace umatiConnect
{
    class Program
    {
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

                // start the server.
                await StartServer(config).ConfigureAwait(false);

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

                //config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);


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
                // start the server.
                using (var server = new UmatiServer())
                {
                    
                    server.Start(config);

                    Console.WriteLine("Server started: {0} at {1}", config.ApplicationName, config.ServerConfiguration.BaseAddresses[0]);
                    Console.WriteLine("Press Ctrl-C to exit...");

                    await Task.Delay(-1).ConfigureAwait(false);
                }
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
    }
}