using OctopusSamples.ProductService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OctopusSamples.ProductService.Repositories
{
    public interface IProductRepository
    {
        Task<List<ProductDetail>> GetAll();
        Task<ProductDetail> GetById(int id);
    }
}
