using Microsoft.AspNetCore.Mvc;
using OctopusSamples.ShoppingCartService.Models;

namespace OctopusSamples.ShoppingCartService.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        [HttpGet("{id}")]
        [ProducesResponseType(404)]
        public ActionResult<ShoppingCart> GetByIdAsync(int id)
        {
            return new ShoppingCart {Id = id};
        }
        
        [HttpPost]
        public IActionResult Checkout(ShoppingCart cart)
        {
            // Create Order from Shopping Cart
            // TODO: Log message for now. 

            return CreatedAtRoute("GetById", new { id = cart.Id }, cart);
        }

        [HttpPut("{id}")]
        public IActionResult Update(long id, ShoppingCart item)
        {
            // TODO: Log message for now. 

            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // TODO: Log message for now.
            return NoContent();
        }
    }
}