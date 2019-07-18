using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace OctopusSamples.OctoPetShop.Models
{
    public class ShoppingCartViewModel
    {
        public ShoppingCartViewModel()
        {
            CartItems = new List<ProductViewModel>();
        }

        public List<ProductViewModel> CartItems { get; private set; }
        
        public string Name { get; set; }
        
        
        public string Street { get; set; }
        public string City { get; set; }
        [DisplayName("Post Code")]
        public string PostCode { get; set; }
        
        [DataType(DataType.CreditCard)]
        [Display(Name = "Credit Card")]
        public string CreditCardNumber { get; set; }
        [Display(Name = "Expiry", Prompt = "MM")]
        public string CreditCardExpiryMonth { get; set; }
        [Display(Name = "", Prompt = "YY")]
        public string CreditCardExpiryYear { get; set; }
        [Display(Name = "Security Code")]
        public string CreditCardSecurityNumber { get; set; }
    }
}