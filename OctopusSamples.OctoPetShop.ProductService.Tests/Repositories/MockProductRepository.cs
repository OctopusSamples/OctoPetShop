using OctopusSamples.ProductService.Models;
using OctopusSamples.ProductService.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctopusSamples.OctoPetShop.ProductService.Tests.Repositories
{
    public class MockProductRepository : IProductRepository
    {
        private readonly ProductDetail[] PetProducts;

        public MockProductRepository(ProductDetail[] productDetails)
        {
            PetProducts = productDetails;
        }

        public Task<List<ProductDetail>> GetAll()
        {
            return Task.Run(() => PetProducts.ToList());
        }

        public Task<ProductDetail> GetById(int id)
        {
            return Task.Run(() => PetProducts.FirstOrDefault(p => p.Id == id));
        }
    }
}
