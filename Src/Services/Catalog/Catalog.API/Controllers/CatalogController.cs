using Catalog.API.Entities;
using Catalog.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "CanRead")]
    public class CatalogController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(IProductRepository productRepository, ILogger<CatalogController> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            //var correlationId = HttpContext.Request.Headers["CorrelationId"];
            //using var loggerScope = _logger.BeginScope("{CorrelationId}", correlationId);

            var products = await _productRepository.GetProducts();
            if (products == null || !products.Any())
            {
                _logger.LogWarning("No products found.");
                return NotFound();
            }

            _logger.LogDebug("Returning all catalog products.");
            return Ok(products);
        }

        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Product>> GetProductById(string id)
        {
            var product = await _productRepository.GetProduct(id);
            if (product == null)
            {
                _logger.LogWarning("Product with id: {ProductId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Returning product with id: {ProductId}", id);
            return Ok(product);
        }

        [Route("[action]/{category}")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductByCategory(string category)
        {
            var products = await _productRepository.GetProductByCategory(category);
            return Ok(products);
        }

        [Route("[action]/{name}")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductByName(string name)
        {
            var products = await _productRepository.GetProductByName(name);
            return Ok(products);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        [Authorize(Roles = "PremiumUser", Policy = "HasFullAccess")]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            await _productRepository.CreateProduct(product);

            return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        [Authorize(Roles = "PremiumUser", Policy = "HasFullAccess")]
        public async Task<IActionResult> UpdateProduct([FromBody] Product product)
        {
            return Ok(await _productRepository.UpdateProduct(product));
        }

        [HttpDelete("{id:length(24)}", Name = "DeleteProduct")]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        [Authorize(Roles = "PremiumUser", Policy = "HasFullAccess")]
        public async Task<IActionResult> DeleteProductById(string id)
        {
            bool productDeleted = await _productRepository.DeleteProduct(id);
            if(productDeleted)
            {
                _logger.LogInformation("Product with id: '{ProductId}' successfully deleted.", id);
                return Ok(productDeleted);
            }
            else
            {
                _logger.LogError("Attempt to delete product with id: '{ProductId}' failed.", id);
                return NoContent();
            }
        }
    }
}
