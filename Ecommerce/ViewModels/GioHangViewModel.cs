using Ecommerce.Models;
using System.Collections.Generic;

namespace Ecommerce.ViewModels
{
    public class GioHangViewModel
    {
        public List<GioHang> CartItems { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
    }
} 
