using Catalog.API.Data;
using Catalog.API.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private const string ProductsCollectionName = "Products";
        private readonly ICatalogContext _context;
        private readonly IMongoCollection<Product> _productsCollection;

        public ProductRepository(ICatalogContext context)
        {
            _context = context;
            _productsCollection = _context.Database.GetCollection<Product>(ProductsCollectionName);
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _productsCollection.AsQueryable().ToListAsync();
        }

        public async Task<Product> GetProduct(string id)
        {
            return await _productsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetProductByCategory(string categoryName)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Category, categoryName);

            return await _productsCollection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductByName(string name)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Name, name);

            return await _productsCollection.Find(filter).ToListAsync();
        }

        public async Task CreateProduct(Product product)
        {
            await _productsCollection.InsertOneAsync(product);
        }

        public async Task<bool> DeleteProduct(string id)
        {
            var deletedProduct = await _productsCollection.DeleteOneAsync(p => p.Id == id);

            return deletedProduct.IsAcknowledged && deletedProduct.DeletedCount > 0;
        }

        public async Task<bool> UpdateProduct(Product product)
        {
            var updatedResult = await _productsCollection.ReplaceOneAsync(filter: p => p.Id == product.Id, replacement: product);

            return updatedResult.IsAcknowledged && updatedResult.ModifiedCount > 0;
        }
    }
}
