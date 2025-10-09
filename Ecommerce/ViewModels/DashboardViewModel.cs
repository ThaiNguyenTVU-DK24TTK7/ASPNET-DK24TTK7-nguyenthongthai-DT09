using System;
using System.Collections.Generic;

namespace Ecommerce.ViewModels
{
    public class DashboardViewModel
    {
        public List<SalesChartPoint> SalesChart { get; set; } = new();
        public List<TopProductViewModel> TopProducts { get; set; } = new();
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
        
        // Summary statistics
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalContacts { get; set; }
        public int NewContacts { get; set; }
    }

    public class SalesChartPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class TopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int SoldCount { get; set; }
    }

    public class RecentOrderViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public DateTime Date { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
} 
