using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.ViewModels
{
    public class AdminAddProductViewModel
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả sản phẩm là bắt buộc")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        public decimal RegularPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn 0")]
        public decimal? SalePrice { get; set; }

        [Required(ErrorMessage = "Chất liệu là bắt buộc")]
        [StringLength(50, ErrorMessage = "Chất liệu không được vượt quá 50 ký tự")]
        public string Material { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thương hiệu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Thương hiệu không được vượt quá 100 ký tự")]
        public string Brand { get; set; } = string.Empty;

        public List<IFormFile>? ProductImages { get; set; }
        public List<ProductSizeViewModel> Sizes { get; set; } = new List<ProductSizeViewModel>();
        public string SizeTemplate { get; set; } = "custom"; // default, custom, clothing, shoes, electronics
    }

    public class ProductSizeViewModel
    {
        public string Size { get; set; } = string.Empty;
        public string? Color { get; set; } = ""; // Cho phép để trống
        public int StockQuantity { get; set; }
        public decimal? RegularPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public bool IsStandard { get; set; }
    }
    public static class SizeTemplates
    {
        public static readonly Dictionary<string, List<string>> Templates = new Dictionary<string, List<string>>
        {
            { "clothing", new List<string> { "S", "M", "L", "XL", "XXL" } },
            { "shoes", new List<string> { "37", "38", "39", "40", "41", "42", "43", "44" } },
            { "custom", new List<string>() }
        };
        
        public static readonly Dictionary<string, string> TemplateNames = new Dictionary<string, string>
        {
            { "clothing", "Quần áo (S, M, L, XL, XXL)" },
            { "shoes", "Giày dép (37-44)" },
            { "custom", "Tùy chỉnh" }
        };
    }
} 
