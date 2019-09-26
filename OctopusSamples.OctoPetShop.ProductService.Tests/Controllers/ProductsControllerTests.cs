using FluentAssertions;
using OctopusSamples.OctoPetShop.ProductService.Tests.Repositories;
using OctopusSamples.ProductService.Controllers;
using OctopusSamples.ProductService.Models;
using System.Threading.Tasks;
using Xunit;

namespace OctopusSamples.OctoPetShop.ProductService.Tests
{
    public class ProductsControllerTests
    {
        private static ProductDetail[] PetProducts = new[]
        {
            new ProductDetail
            {
                Id = 1,
                Name = "Dog",
                Description = "Black and white Border Collie puppy",
                ImageUrl = "https://s3.amazonaws.com/i.octopus.com/videos/Dog.png",
                Price = 100.0,
                InStock = true
            },
            new ProductDetail
            {
                Id = 2,
                Name = "Cat",
                Description = "Friendly kitten",
                ImageUrl = "https://s3.amazonaws.com/i.octopus.com/videos/Cat.png",
                Price = 75.0,
                InStock = false
            },
            new ProductDetail
            {
                Id = 4,
                Name = "Fish",
                Description = "Goldie the goldfish",
                ImageUrl = "https://s3.amazonaws.com/i.octopus.com/videos/Fish.png",
                Price = 15.0,
                InStock = true
            },
            new ProductDetail
            {
                Id = 4,
                Name = "Octopus",
                Description = "The amazing wonder of the ocean",
                ImageUrl = "https://s3.amazonaws.com/i.octopus.com/videos/Octopus.png",
                Price = 250.0,
                InStock = true
            },
        };

        [Fact]
        public async Task GetAllShouldReturnAllProducts()
        {
            // Arrange
            var controller = new ProductsController(new MockProductRepository(PetProducts));

            // Act
            var result = await controller.GetAllAsync();

            // Assert
            result.Value.Should().BeEquivalentTo(PetProducts);
        }

        [Fact]
        public async Task GetByIdShouldReturnProductWithMatchingId()
        {
            // Arrange
            var controller = new ProductsController(new MockProductRepository(PetProducts));

            // Act
            var result = await controller.GetByIdAsync(2);

            // Assert
            result.Value.Should().BeEquivalentTo(PetProducts[1]);
        }


        [Fact]
        public async Task GetByIdShouldReturnNullIfNoProductHasMatchingId()
        {
            // Arrange
            var controller = new ProductsController(new MockProductRepository(PetProducts));

            // Act
            var result = await controller.GetByIdAsync(-1);

            // Assert
            result.Value.Should().BeNull();
        }
    }
}
