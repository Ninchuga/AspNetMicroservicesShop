using Destructurama;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System;

namespace Shopping.Logging
{
    public static class LoggingConfiguration
    {
        
        /// <summary>
        /// Used in Program.cs to configure logger on host builder 
        /// </summary>
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
            (hostingContext, loggerConfiguration) =>
            {
                var env = hostingContext.HostingEnvironment;
                var levelSwitch = new LoggingLevelSwitch();

                if (hostingContext.HostingEnvironment.IsDevelopment())
                {
                    //loggerConfiguration.MinimumLevel.Override("*.API", LogEventLevel.Debug);
                    levelSwitch.MinimumLevel = LogEventLevel.Debug; // place this maybe in appsettings
                }

                loggerConfiguration
                    .MinimumLevel.ControlledBy(levelSwitch) // <= default minimum level
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("ApplicationName", env.ApplicationName)
                    .Enrich.WithProperty("EnvironmentName", env.EnvironmentName)
                    .Enrich.WithExceptionDetails()
                    .Enrich.FromMassTransitMessage()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // <= overrides namespaces to log from Warning to higher (debug, info will be skipped)
                    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                    .WriteTo.Console()
                    .Destructure.UsingAttributes();

                var elasticUrl = hostingContext.Configuration.GetValue<string>("Logging:ElasticUrl");
                if(!string.IsNullOrWhiteSpace(elasticUrl))
                {
                    loggerConfiguration.WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri(elasticUrl))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                            IndexFormat = "shopping-logs-{0:dd.MM.yyyy}", // <= new index will be generated each day
                            MinimumLogEventLevel = LogEventLevel.Debug
                        });
                }
            };
    }
}
