namespace OctopusSamples.ProductService.Models
{
    public class ProductDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool InStock { get; set; }
        public double Price { get; set; }
    }
}