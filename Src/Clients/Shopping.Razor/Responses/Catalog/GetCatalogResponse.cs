using Shopping.Razor.Models;
using System.Collections.Generic;

namespace Shopping.Razor.Responses.Catalog
{
    public class GetCatalogResponse : BaseResponse
    {
        public IReadOnlyList<CatalogItem> CatalogItems { get; set; }
    }
}
