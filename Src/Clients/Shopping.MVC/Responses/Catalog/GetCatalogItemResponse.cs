using Shopping.MVC.Models;

namespace Shopping.MVC.Responses.Catalog
{
    public class GetCatalogItemResponse : BaseResponse
    {
        public CatalogItem CatalogItem { get; set; }
    }
}
