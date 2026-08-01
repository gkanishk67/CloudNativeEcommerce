using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductDataService _productDataService;

        public ProductsController(IProductDataService productDataService)
        {
            _productDataService = productDataService;
        }

        [HttpGet]
        public async Task<List<Product>> Get()
        {
            return await _productDataService.GetAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            await _productDataService.CreateAsync(product);

            return Ok(product);
        }
    }
}