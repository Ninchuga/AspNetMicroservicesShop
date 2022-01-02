using Shopping.Razor.Models;

namespace Shopping.Razor.Responses.Catalog
{
    public class GetCatalogItemResponse : BaseResponse
    {
        public CatalogItem CatalogItem { get; set; }
    }
}
