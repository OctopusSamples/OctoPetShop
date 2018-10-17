using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OctopusSamples.ProductService.Models;

namespace OctopusSamples.ProductService.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private static readonly ProductDetail[] PetProducts = new[]
        {
            new ProductDetail
            {
                Id = 1,
                Name = "Dog",
                Description = "Black and white border collie",
                ImageUrl = "https://"
            }
        };
        
        [HttpGet]
        public async Task<ActionResult<List<ProductDetail>>> GetAllAsync()
        {
            return await Task.Run(() => PetProducts.ToList());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductDetail>> GetByIdAsync(int id)
        {
            var pet = await Task.Run(() => PetProducts.FirstOrDefault(x => x.Id == id));

            if (pet == null)
            {
                return NotFound();
            }

            return pet;
        }
    }
}