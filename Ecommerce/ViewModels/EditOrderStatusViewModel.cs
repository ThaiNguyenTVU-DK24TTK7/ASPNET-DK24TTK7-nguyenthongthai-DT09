using System;
using System.Collections.Generic;

namespace Ecommerce.ViewModels
{
    public class EditOrderStatusViewModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;

        // Thông tin đơn hàng
        public string? CustomerName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<EditOrderProductViewModel> Products { get; set; } = new();
    }

    public class EditOrderProductViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
} 
