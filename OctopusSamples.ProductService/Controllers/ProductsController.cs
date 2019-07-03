using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OctopusSamples.ProductService.Models;
using OctopusSamples.ProductService.Repositories;

namespace OctopusSamples.ProductService.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        private static readonly ProductDetail[] PetProducts = new[]
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
        
        [HttpGet]
        public async Task<ActionResult<List<ProductDetail>>> GetAllAsync()
        {
            return await _productRepository.GetAll(); //Task.Run(() => PetProducts.ToList());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductDetail>> GetByIdAsync(int id)
        {
            var pet = await _productRepository.GetById(id); // Task.Run(() => PetProducts.FirstOrDefault(x => x.Id == id));

            if (pet == null)
            {
                return NotFound();
            }

            return pet;
        }
    }
}