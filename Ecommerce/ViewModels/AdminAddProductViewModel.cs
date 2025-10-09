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

        public List<IFormFile>? ProductImages { get; set; }

        // Kích thước và số lượng
        public List<ProductSizeViewModel> Sizes { get; set; } = new List<ProductSizeViewModel>
        {
            new ProductSizeViewModel { Size = "S", StockQuantity = 0 },
            new ProductSizeViewModel { Size = "M", StockQuantity = 0 },
            new ProductSizeViewModel { Size = "L", StockQuantity = 0 },
            new ProductSizeViewModel { Size = "XL", StockQuantity = 0 },
            new ProductSizeViewModel { Size = "XXL", StockQuantity = 0 }
        };
    }

    public class ProductSizeViewModel
    {
        public string Size { get; set; } = string.Empty;
        public string? Color { get; set; } = ""; // Cho phép để trống
        public int StockQuantity { get; set; }
    }
} 
