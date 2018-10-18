using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OctopusSamples.OctoPetShop.Controllers.Client;
using OctopusSamples.OctoPetShop.Models;

namespace OctopusSamples.OctoPetShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductClient _productClient;

        public HomeController(IProductClient productClient)
        {
            _productClient = productClient;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productClient.GetAsync(CancellationToken.None);
            return View(products);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        public async Task<IActionResult> ShoppingCart()
        {
            var products = await _productClient.GetAsync(CancellationToken.None);
            
            var shoppingCartViewModel = new ShoppingCartViewModel();
            shoppingCartViewModel.CartItems.Add(products.First());
            
            return View(shoppingCartViewModel);
        }

        [HttpPost]
        public IActionResult AddToCart(int id)
        {
            // TODO: Call Cart Service and Save Items 
            return RedirectToAction("ShoppingCart");
        }
        
        [HttpPost]
        public IActionResult Checkout(ShoppingCartViewModel cart)
        {
            // TODO: Redirect to Order Confirmation / Thank you page 
            return RedirectToAction("ShoppingCart");
        }
        
    }
}