using Shopping.Aggregator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shopping.Aggregator.Contracts
{
    public interface ICatalogService
    {
        Task<IReadOnlyCollection<CatalogModel>> GetCatalog();
        Task<IReadOnlyCollection<CatalogModel>> GetCatalogProductsByCategory(string category);
        Task<CatalogModel> GetCatalogProductBy(string productId);
    }
}
