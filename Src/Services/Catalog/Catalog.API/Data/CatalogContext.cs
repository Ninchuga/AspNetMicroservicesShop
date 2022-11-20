using Catalog.API.Entities;
using Catalog.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext
    {
        public CatalogContext(IOptions<MongoDBSettings> mongoDbSettings)
        {
            var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
            Database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);

            CatalogContextSeed.SeedData(Database.GetCollection<Product>(mongoDbSettings.Value.ProductsCollectionName));
        }

        public IMongoDatabase Database { get; }
    }
}
