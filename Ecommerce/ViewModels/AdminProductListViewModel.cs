using System;

namespace Ecommerce.ViewModels
{
    public class AdminProductListViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int SoldCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 
