using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using OctopusSamples.ProductService.Models;

namespace OctopusSamples.ProductService.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IConfiguration _configuration;

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_configuration.GetConnectionString("OPSConnectionString"));
            }
        }

        public ProductRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<ProductDetail>> GetAll()
        {
            using (var connection = Connection)
            {
                connection.Open();

                var query = "SELECT Id, Name, Description, ImageUrl, Price, InStock FROM Products";
                var result = await connection.QueryAsync<ProductDetail>(query);

                return result.ToList();
            }
        }

        public async Task<ProductDetail> GetById(int id)
        {
            using (var connection = Connection)
            {
                connection.Open();

                var query = "SELECT Id, Name, Description, ImageUrl, Price, InStock FROM Products WHERE Id = @Id";
                var result = await connection.QueryAsync<ProductDetail>(query, new { Id = id });

                return result.FirstOrDefault();
            }
        }
    }
}
