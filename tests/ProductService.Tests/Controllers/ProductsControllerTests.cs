using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductService.Controllers;
using ProductService.Models;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests.Controllers
{
    public class ProductsControllerTests
    {
        /// <summary>
        /// Get_ShouldReturnAllProducts
        /// </summary>
        [Fact]
        public async Task Get_ShouldReturnAllProducts()
        {
            // Arrange
            var mockService = new Mock<IProductDataService>();

            var products = new List<Product>
            {
                new Product
                {
                    Id = "1",
                    Name = "Laptop",
                    Price = 1000,
                    Stock = 10
                },
                new Product
                {
                    Id = "2",
                    Name = "Mouse",
                    Price = 20,
                    Stock = 50
                }
            };

            mockService
                .Setup(x => x.GetAsync())
                .ReturnsAsync(products);

            var controller =
                new ProductsController(mockService.Object);

            // Act
            var result = await controller.Get();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Laptop", result[0].Name);
            Assert.Equal("Mouse", result[1].Name);
        }

        /// <summary>
        /// Create_ShouldReturnOkResult
        /// </summary>
        [Fact]
        public async Task Create_ShouldReturnOkResult()
        {
            // Arrange
            var mockService = new Mock<IProductDataService>();

            var product = new Product
            {
                Name = "Keyboard",
                Price = 100,
                Stock = 20
            };

            mockService
                .Setup(x => x.CreateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var controller =
                new ProductsController(mockService.Object);

            // Act
            var result = await controller.Create(product);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedProduct =
                Assert.IsType<Product>(okResult.Value);

            Assert.Equal("Keyboard", returnedProduct.Name);

            mockService.Verify(
                x => x.CreateAsync(It.IsAny<Product>()),
                Times.Once);
        }
    }
}