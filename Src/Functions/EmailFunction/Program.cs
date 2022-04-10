using EmailFunction.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Azure;
using EmailFunction.Services;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace EmailFunction
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    //var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
                    config.AddAzureKeyVault(new SecretClient(new Uri(Environment.GetEnvironmentVariable("KeyVaultUri")), new DefaultAzureCredential()), new KeyVaultSecretManager());
                })
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    worker.UseNewtonsoftJson();
                })
                .ConfigureServices(services =>
                {
                    services.AddAzureClients(builder =>
                    {
                        // Add a KeyVault client (SecretClient)
                        builder.AddSecretClient(new Uri(Environment.GetEnvironmentVariable("KeyVaultUri")));
                    });

                    services.AddSingleton<IKeyVaultManager, KeyVaultManager>();
                })
                //.UseSerilog(LoggingConfiguration.Configure)
                .Build();

            await host.RunAsync();
        }
    }
}