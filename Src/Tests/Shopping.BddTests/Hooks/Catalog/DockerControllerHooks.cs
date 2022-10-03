using Ductus.FluentDocker.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Ductus.FluentDocker.Builders;
using System.Net;
using BoDi;
using System.Net.Http;

namespace Shopping.BddTests.Hooks.Catalog
{
    [Binding]
    public class DockerControllerHooks
    {
        private static ICompositeService _compositeService;
        private IObjectContainer _objectCotainer; // DI container in SpecFlow. With it we can add dependencies in the Steps

        public DockerControllerHooks(IObjectContainer objectCotainer)
        {
            _objectCotainer = objectCotainer;
        }

        [BeforeTestRun]
        public static void DockerComposeUp()
        {
            var config = LoadConfiguration();

            var dockerComposeFileName = config["DockerComposeFileName"];
            var dockerComposePath = GetDockerComposeLocation(dockerComposeFileName);

            var confirmationUrl = config["CatalogApiUrl"];
            _compositeService = new Builder()
                .UseContainer()
                .UseCompose()
                .FromFile(dockerComposePath)
                .RemoveOrphans()
                .ForceRecreate()
                //.WaitForHttp("catalog.api", $"{confirmationUrl}/api/v1/Catalog",
                //    continuation: (response, _) => response.Code != HttpStatusCode.OK ? 2000 : 0)
                .Build()
                .Start();
        }

        [AfterTestRun]
        public static void DockerComposeDown()
        {
            _compositeService.Stop();
            _compositeService.Dispose();
        }

        [BeforeScenario()]
        public void AddHttpClient()
        {
            var config = LoadConfiguration();
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(config["CatalogApiUrl"])
            };
            _objectCotainer.RegisterInstanceAs(httpClient);
        }

        private static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        private static string GetDockerComposeLocation(string dockerComposeFileName)
        {
            var directory = Directory.GetCurrentDirectory();
            while (!Directory.EnumerateFiles(directory, "*.yml").Any(s => s.EndsWith(dockerComposeFileName)))
            {
                directory = directory.Substring(0, directory.LastIndexOf(Path.DirectorySeparatorChar));
            }

            return Path.Combine(directory, dockerComposeFileName);

        }
    }
}
