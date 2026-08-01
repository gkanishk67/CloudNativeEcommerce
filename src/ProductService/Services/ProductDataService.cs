using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProductService.Configurations;
using ProductService.Models;

namespace ProductService.Services
{
    public class ProductDataService : IProductDataService
    {
        private readonly IMongoCollection<Product> _productsCollection;
        private readonly ILogger<ProductDataService> _logger;

        public ProductDataService(
            IOptions<MongoDbSettings> mongoDbSettings, ILogger<ProductDataService> logger)
        {
            var mongoClient = new MongoClient(
                mongoDbSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                mongoDbSettings.Value.DatabaseName);

            _productsCollection = mongoDatabase.GetCollection<Product>(
                mongoDbSettings.Value.ProductsCollectionName);
            _logger = logger;
        }

        public async Task<List<Product>> GetAsync() =>
            await _productsCollection.Find(_ => true).ToListAsync();

        public async Task CreateAsync(Product product)
        {
            await _productsCollection.InsertOneAsync(product);
            _logger.LogInformation(
            "Product {ProductName} created",
            product.Name);
        }

        public async Task<Product?> GetByNameAsync(string name)
        {
            return await _productsCollection
                .Find(x => x.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            await _productsCollection.ReplaceOneAsync(
                x => x.Id == product.Id,
                product);
        }
    }
}