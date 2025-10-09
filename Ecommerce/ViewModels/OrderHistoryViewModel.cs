using Ecommerce.Models;
using System.Collections.Generic;

namespace Ecommerce.ViewModels
{
    public class OrderHistoryViewModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public bool IsReviewed { get; set; }
    }
} 
