using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using BoDi;
using System.Net.Http;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Networks;
using Shopping.BddTests.Models.Catalog;

namespace Shopping.BddTests.Hooks.Catalog
{
    [Binding]
    public class DockerControllerHooks
    {
        private static readonly CatalogApiImage _catalogApiImage = new();
        private static readonly IdentityProviderImage _identityProviderImage = new();
        private static TestcontainerDatabase _catalogDbContainer;
        private static TestcontainerDatabase _identityDbContainer;
        private static IDockerNetwork _dockerNetwork;
        private static IDockerContainer _catalogApiContainer;
        private static IDockerContainer _identityProviderContainer;
        private IObjectContainer _objectCotainer; // DI container in SpecFlow. With it we can add dependencies in the Steps
        private static IConfiguration _configuration;

        public DockerControllerHooks(IObjectContainer objectCotainer)
        {
            _objectCotainer = objectCotainer;
        }

        [BeforeTestRun]
        public static async Task CreateAndStartDockerContainers()
        {
            _configuration = LoadConfiguration();

            await BuildAndCreateDockerNetwork().ConfigureAwait(false);

            string identityDbContainerName = "identitydbtest";
            await BuildAndStartIdentityDbContainer(identityDbContainerName).ConfigureAwait(false);

            await BuildAndStartCatalogDbContainer().ConfigureAwait(false);

            //await _catalogApiImage.InitializeAsync()
            //    .ConfigureAwait(false);

            //await BuildAndStartCatalogApiContainer().ConfigureAwait(false);

            await BuildAndStartIdentityContainer(identityDbContainerName).ConfigureAwait(false);
        }

        private static async Task BuildAndStartCatalogDbContainer()
        {
            _catalogDbContainer = new TestcontainersBuilder<MongoDbTestcontainer>()
                            .WithDatabase(new MongoDbTestcontainerConfiguration
                            {
                                Database = "CatalogDb",
                                Username = null,
                                Password = null
                            })
                            .WithImage("mongo")
                            .WithNetwork(_dockerNetwork)
                            .WithName("catalogdbtest")
                            .WithPortBinding(27017, 27017)
                            .Build();

            await _catalogDbContainer.StartAsync()
              .ConfigureAwait(false);
        }

        private static async Task BuildAndStartIdentityDbContainer(string identityDbContainerName)
        {
            _identityDbContainer = new TestcontainersBuilder<MsSqlTestcontainer>()
                        .WithDatabase(new MsSqlTestcontainerConfiguration
                        {
                            Password = "September24#",
                            Database = "IdentityDb"
                        })
                        .WithNetwork(_dockerNetwork)
                        .WithName(identityDbContainerName)
                        .Build();

            await _identityDbContainer.StartAsync()
                .ConfigureAwait(false);
        }

        private static async Task BuildAndCreateDockerNetwork()
        {
            _dockerNetwork = new TestcontainersNetworkBuilder()
                            .WithName(Guid.NewGuid().ToString("D"))  // Use random names to prevent name clashes.
                            .Build();

            await _dockerNetwork.CreateAsync()
              .ConfigureAwait(false);
        }

        private static async Task BuildAndStartIdentityContainer(string identityDbContainerName)
        {
            _identityProviderContainer = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage(_identityProviderImage)
                .WithNetwork(_dockerNetwork)
                .WithName("identityprovidertest")
                .WithExposedPort(IdentityProviderImage.HttpsPort)
                .WithPortBinding(8021, IdentityProviderImage.HttpsPort)
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                .WithEnvironment("ASPNETCORE_URLS", "https://+")
                .WithEnvironment("ASPNETCORE_HTTPS_PORT", "8000")
                .WithEnvironment("IdentityIssuer", _configuration["IdentityProviderUrl"])
                .WithEnvironment("ConnectionStrings:IdentityDb", BuildIdentityDbConnectionString(identityDbContainerName))
                .WithEnvironment("WebClientUrls:CatalogApi", _configuration["CatalogApiUrl"])
                .WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Path", IdentityProviderImage.CertificateContainerFilePath)
                .WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Password", IdentityProviderImage.CertificatePassword)
                .WithBindMount(IdentityProviderImage.IdentityProviderRootPath, "/root/Identity")
                .WithBindMount(IdentityProviderImage.RootCertificateHostAbsoluteFilePath, "/https-root/shopping-root-cert.cer")
                .WithBindMount($"{IdentityProviderImage.IdentityProviderCertificatesHostFilePath}/Shopping.IDP.pfx", IdentityProviderImage.CertificateContainerFilePath)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(IdentityProviderImage.HttpsPort))
                .Build();

            await _identityProviderContainer.StartAsync()
                .ConfigureAwait(false);
        }

        private static string BuildIdentityDbConnectionString(string identityDbContainerName) =>
            $"Server={identityDbContainerName};Database={_identityDbContainer.Database};User Id={_identityDbContainer.Username};Password={_identityDbContainer.Password};TrustServerCertificate=True;";

        private static async Task BuildAndStartCatalogApiContainer()
        {
            _catalogApiContainer = new TestcontainersBuilder<TestcontainersContainer>()
                  .WithImage(_catalogApiImage)
                  .WithNetwork(_dockerNetwork)
                  .WithName("catalogapitest")
                  .WithExposedPort(CatalogApiImage.HttpsPort)
                  .WithPortBinding(8000, CatalogApiImage.HttpsPort)
                  .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                  .WithEnvironment("ASPNETCORE_URLS", "https://+")
                  .WithEnvironment("ASPNETCORE_HTTPS_PORT", "8000")
                  .WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Path", CatalogApiImage.CertificateContainerFilePath)
                  .WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Password", CatalogApiImage.CertificatePassword)
                  .WithEnvironment("DatabaseSettings:ConnectionString", "mongodb://catalogdbtest:27017")
                  .WithEnvironment("IdentityProviderSettings:IdentityServiceUrl", "https://host.docker.internal:8021")
                  .WithBindMount(CatalogApiImage.CatalogApiRootPath, "/root/Catalog")
                  .WithBindMount(CatalogApiImage.RootCertificateHostAbsoluteFilePath, "/https-root/shopping-root-cert.cer")
                  .WithBindMount($"{CatalogApiImage.CatalogApiCertificatesHostFilePath}\\Catalog.API.pfx", CatalogApiImage.CertificateContainerFilePath)
                  .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(CatalogApiImage.HttpsPort))
                  .Build();

            await _catalogApiContainer.StartAsync()
              .ConfigureAwait(false);
        }

        [AfterTestRun]
        public static async Task DockerDisposeContainersAndImages()
        {
            // Catalog Api
            //await _catalogApiContainer.DisposeAsync()
            //  .ConfigureAwait(false);

            //await _catalogDbContainer.DisposeAsync()
            //  .ConfigureAwait(false);

            //await _catalogApiImage.DisposeAsync()
            //    .ConfigureAwait(false);

            //// Identity provider
            //await _identityProviderContainer.DisposeAsync()
            //    .ConfigureAwait(false);

            //await _identityDbContainer.DisposeAsync()
            //    .ConfigureAwait(false);

            //await _identityProviderImage.DisposeAsync()
            //    .ConfigureAwait(false);

            //await _dockerNetwork.DeleteAsync()
            //  .ConfigureAwait(false);
        }

        [BeforeScenario()]
        public void AddHttpClient()
        {
            // skip validation of certificate
            //var handler = new HttpClientHandler
            //{
            //    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            //};

            //var catalogApiHttpClient = new HttpClient(handler)
            //{
            //    BaseAddress = new Uri(_configuration["CatalogApiUrl"])
            //};

            //var identityProviderHttpClient = new HttpClient(handler)
            //{
            //    BaseAddress = new Uri(_configuration["IdentityProviderUrl"])
            //};

            //_objectCotainer.RegisterInstanceAs(_configuration, name: "configuration");
            //_objectCotainer.RegisterInstanceAs(catalogApiHttpClient, name: "catalogApiHttpClient");
            //_objectCotainer.RegisterInstanceAs(identityProviderHttpClient, name: "identityProviderHttpClient");
        }

        private static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }
    }
}
