using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Console
{
    class Program
    {
        static async Task Main()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var caFile = Path.Combine(baseDir, "rootCA.pem");
            var certFile = Path.Combine(baseDir, "mongodb.pem");

            // https://docs.mongodb.com/manual/reference/connection-string/

            // connect with ssl info in the connection string
            await ConnectUsingSslConnectionString(caFile);

            // connect using the MongoClientSettings
            await ConnectUsingClientSettings(certFile);

            // cannot connect without specifying Certs
            await ConnectUsingConnectionString();

            System.Console.WriteLine("Press enter to exit");
            System.Console.ReadLine();
        }

        private static async Task ConnectUsingConnectionString()
        {
            try
            {
                var tlsClient = new MongoClient("mongodb://localhost");

                // list the databases to prove we are connected
                var databases = await tlsClient.ListDatabaseNamesAsync();
                var names = await databases.ToListAsync();  

                System.Console.WriteLine($"Connected using connection string: {string.Join(", ", names)}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Unable to connect using connection string: {ex.Message}");
            }
        }

        private static async Task ConnectUsingSslConnectionString(string caFile)
        {
            // disable remote certificate validation for this sample
            var tlsClient = new MongoClient(
                $"mongodb://localhost?tls=true&tls_ca_certs={caFile}&tlsInsecure=true");

            // list the databases to prove we are connected
            var databases = await tlsClient.ListDatabaseNamesAsync();
            var names = await databases.ToListAsync();  

            System.Console.WriteLine($"Connected using SSL connection string: {string.Join(", ", names)}");
        }

        private static async Task ConnectUsingClientSettings(string certFile)
        {
            var clientSettings = new MongoClientSettings();
            clientSettings.SslSettings = new SslSettings();
            clientSettings.UseTls = true;

            clientSettings.SslSettings.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            var cert = new X509Certificate2(certFile);
            clientSettings.SslSettings.ClientCertificateSelectionCallback =
                (sender, host, certificates, certficate, issuer) => cert;

            // disable remote certificate validation for this sample
            clientSettings.SslSettings.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;

            var tlsClient = new MongoClient(clientSettings);

            // list the databases to prove we are connected
            var databases = await tlsClient.ListDatabaseNamesAsync();
            var names = await databases.ToListAsync();  

            System.Console.WriteLine($"Connected using MongoClientSettings: {string.Join(", ", names)}");
        }
    }
}
