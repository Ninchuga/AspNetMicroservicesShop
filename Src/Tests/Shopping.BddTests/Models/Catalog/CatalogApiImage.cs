﻿using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shopping.BddTests.Models.Catalog
{
    public class CatalogApiImage : IDockerImage, IAsyncLifetime
    {
        public const ushort HttpsPort = 443;

        public const ushort HttpsExposedPort = 8000;

        public const string CertificateContainerFilePath = "/https/Catalog.API.pfx";

        public static string CatalogApiRootPath = $"{Path.Combine(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, @"Services/Catalog/Catalog.API")}";

        public static string CatalogApiCertificatesHostFilePath = $"{CatalogApiRootPath}/certs";

        public static string RootCertificateHostAbsoluteFilePath = $"{Path.Combine(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, @"certs/shopping-root-cert.cer")}";

        public const string CertificatePassword = "password";

        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        private readonly IDockerImage _image = new DockerImage(string.Empty, "catalogapi", "latest");

        public string Repository => _image.Repository;

        public string Name => _image.Name;

        public string Tag => _image.Tag;

        public string FullName => _image.FullName;

        public async Task InitializeAsync()
        {
            await _semaphoreSlim.WaitAsync()
                .ConfigureAwait(false);

            try
            {
                _ = await new ImageFromDockerfileBuilder()
                  .WithName("catalogapi")
                  .WithDockerfileDirectory(CatalogApiRootPath)
                  .WithDockerfile("Dockerfile")
                  .WithBuildArgument("RESOURCE_REAPER_SESSION_ID", ResourceReaper.DefaultSessionId.ToString("D")) // https://github.com/testcontainers/testcontainers-dotnet/issues/602.
                  .WithDeleteIfExists(false)
                  .Build()
                  .ConfigureAwait(false);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public string GetHostname()
        {
            return _image.GetHostname();
        }

        
    }
}
