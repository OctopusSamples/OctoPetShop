using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OctopusSamples.OctoPetShop.Models;

namespace OctopusSamples.OctoPetShop.Controllers.Client
{
    public interface IProductClient
    {
        Task<List<ProductViewModel>> GetAsync(CancellationToken cancellationToken);
    }

    public class ProductClient : IProductClient
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public ProductClient(IOptions<AppSettings> appSettings)
        {
            _httpClient.BaseAddress = new Uri(appSettings.Value.ProductServiceBaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<ProductViewModel>> GetAsync(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync("api/products", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var stringResult = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ProductViewModel>>(stringResult);
            }
            return new List<ProductViewModel>();
        }
    }
}
