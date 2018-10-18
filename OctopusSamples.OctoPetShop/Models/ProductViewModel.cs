namespace OctopusSamples.OctoPetShop.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool InStock { get; set; }
        
        public double Price { get; set; }
        public string PriceString  => $"${Price}"; // Data Annotations didn't work
    }
}