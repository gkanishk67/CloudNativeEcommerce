using ProductService.Models;

namespace ProductService.Services
{
    public interface IProductDataService
    {
        Task<List<Product>> GetAsync();

        Task CreateAsync(Product product);

        Task<Product?> GetByNameAsync(string name);

        Task UpdateAsync(Product product);
    }
}