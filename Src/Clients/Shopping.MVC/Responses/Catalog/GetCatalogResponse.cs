using Shopping.MVC.Models;
using System.Collections.Generic;

namespace Shopping.MVC.Responses.Catalog
{
    public class GetCatalogResponse : BaseResponse
    {
        public IReadOnlyList<CatalogItem> CatalogItems { get; set; }
    }
}
