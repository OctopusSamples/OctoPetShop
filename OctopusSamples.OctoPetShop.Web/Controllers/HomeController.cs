using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OctopusSamples.OctoPetShop.Controllers.Client;
using OctopusSamples.OctoPetShop.Models;

namespace OctopusSamples.OctoPetShop.Controllers
{
    public class HomeController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IProductClient _productClient;
        private static ShoppingCartViewModel shoppingCartViewModel;

        public HomeController(IProductClient productClient)
        {
            _productClient = productClient;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productClient.GetAsync(CancellationToken.None);
            shoppingCartViewModel = new ShoppingCartViewModel();
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

        public IActionResult Confirmation()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        public IActionResult ShoppingCart()
        {
            
            return View(shoppingCartViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int id)
        {
            var products = await _productClient.GetAsync(CancellationToken.None);

            var productItemSearch = from product in products
                                    where product.Id == id
                                    select product;

            var productItem = productItemSearch.First();

            shoppingCartViewModel.CartItems.Add(productItem);

            return RedirectToAction("ShoppingCart");
        }
        
        [HttpPost]
        public IActionResult Checkout(ShoppingCartViewModel cart)
        {
            // TODO: Redirect to Order Confirmation / Thank you page 
            shoppingCartViewModel.CartItems.Clear();

            return RedirectToAction("Confirmation");
        }

        [HttpPost]
        public IActionResult Finish()
        {
            return RedirectToAction("");
        }

    }
}