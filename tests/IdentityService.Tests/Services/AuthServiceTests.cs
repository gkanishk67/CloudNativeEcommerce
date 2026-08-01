using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace IdentityService.Tests.Services
{
    public class AuthServiceTests
    {
        /// <summary>
        /// MethodName_ShouldExpectedBehavior
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegisterAsync_ShouldCreateUser()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;

            var context = new ApplicationDbContext(options);

            var configuration = new ConfigurationBuilder().Build();

            var authService = new AuthService(
                context,
                configuration,
                NullLogger<AuthService>.Instance);

            var request = new RegisterRequest
            {
                Name = "John",
                Email = "john@test.com",
                Password = "Password123"
            };

            ///Act
            await authService.RegisterAsync(request);

            //Assert
            Assert.Single(context.Users);

            var user = context.Users.First();

            Assert.Equal("John", user.Name);
            Assert.Equal("john@test.com", user.Email);
            Assert.NotEqual("Password123", user.Password);

            //Arrange
            //Act
            //Assert
        }

        /// <summary>
        /// LoginAsync should return null when email does not exist.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Arrange

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
            { "Jwt:Key", "THIS_IS_MY_SUPER_SECRET_KEY_12345" },
            { "Jwt:Issuer", "IdentityService" },
            { "Jwt:Audience", "CloudNativeEcommerce" }
                })
                .Build();

            var authService = new AuthService(
                context,
                configuration,
                NullLogger<AuthService>.Instance);
            // Seed a user into the in-memory database

            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                Email = "john@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123")
            });

            await context.SaveChangesAsync();

            // Create a login request with an email that doesn't exist

            var request = new LoginRequest
            {
                Email = "abc@test.com",
                Password = "Password123"
            };

            // Act

            var result = await authService.LoginAsync(request);

            // Assert

            Assert.Null(result);
        }

        /// <summary>
        /// LoginAsync should return null when password is incorrect.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
            { "Jwt:Key", "THIS_IS_MY_SUPER_SECRET_KEY_12345" },
            { "Jwt:Issuer", "IdentityService" },
            { "Jwt:Audience", "CloudNativeEcommerce" }
                })
                .Build();

            var authService = new AuthService(
                context,
                configuration,
                NullLogger<AuthService>.Instance);
            // Seed a user

            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                Email = "john@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123")
            });

            await context.SaveChangesAsync();

            // Login request with wrong password

            var request = new LoginRequest
            {
                Email = "john@test.com",
                Password = "WrongPassword"
            };

            // Act

            var result = await authService.LoginAsync(request);

            // Assert

            Assert.Null(result);
        }

        /// <summary>
        /// LoginAsync should return a JWT token when credentials are valid.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
            { "Jwt:Key", "THIS_IS_MY_SUPER_SECRET_KEY_12345" },
            { "Jwt:Issuer", "IdentityService" },
            { "Jwt:Audience", "CloudNativeEcommerce" }
                })
                .Build();

            var authService = new AuthService(
                context,
                configuration,
                NullLogger<AuthService>.Instance);
            // Seed a user

            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                Email = "john@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123")
            });

            await context.SaveChangesAsync();

            // Valid login request

            var request = new LoginRequest
            {
                Email = "john@test.com",
                Password = "Password123"
            };

            // Act

            var result = await authService.LoginAsync(request);

            // Assert

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result));
        }
    }
}
