using EmailFunction.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace EmailFunction
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    worker.UseNewtonsoftJson();
                })
                .ConfigureAppConfiguration(configuration =>
                {
                    var settings = configuration.Build();
                    var env = settings["ASPNETCORE_ENVIRONMENT"];

                    configuration.AddJsonFile("local.settings.json", optional: true);
                    //configuration.AddJsonFile($"appsettings.{env}.json", optional: true);
                    //configuration.AddJsonFile($"appsettings.local.json", optional: true);
                    configuration.AddEnvironmentVariables();
                })
                //.UseSerilog(LoggingConfiguration.Configure)
                .Build();

            await host.RunAsync();
        }
    }
}