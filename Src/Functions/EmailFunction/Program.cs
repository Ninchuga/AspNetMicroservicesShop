using EmailFunction.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace EmailFunction
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configuration =>
                {
                    var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

                    // in the local environment function adds by default json file local.settings.json
                    //configuration.AddJsonFile($"secrets.settings.json", optional: true, reloadOnChange: true)
                             //    .AddEnvironmentVariables();
                })
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    worker.UseNewtonsoftJson();
                })
                //.ConfigureServices(services =>
                //{
                //    // add dependencies in here if there are any
                //})
                //.UseSerilog(LoggingConfiguration.Configure)
                .Build();

            await host.RunAsync();
        }
    }
}