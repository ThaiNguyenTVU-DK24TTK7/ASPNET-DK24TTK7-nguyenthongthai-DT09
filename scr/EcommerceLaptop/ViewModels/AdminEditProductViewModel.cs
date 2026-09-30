using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.ViewModels
{
    public class AdminEditProductViewModel
    {
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự")]
        public string ProductName { get; set; } = "";
        
        [Required(ErrorMessage = "Mô tả sản phẩm là bắt buộc")]
        public string Description { get; set; } = "";
        
        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }
        
        [Required(ErrorMessage = "Chất liệu là bắt buộc")]
        [StringLength(50, ErrorMessage = "Chất liệu không được vượt quá 50 ký tự")]
        public string Material { get; set; } = "";

        [Required(ErrorMessage = "Thương hiệu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Thương hiệu không được vượt quá 100 ký tự")]
        public string Brand { get; set; } = "";
        
        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        public decimal RegularPrice { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn 0")]
        public decimal? SalePrice { get; set; }
        
        public List<string> Tags { get; set; } = new();
        public string? MainImageUrl { get; set; }
        public int? MainImageId { get; set; }
        public List<GalleryImageViewModel> GalleryImages { get; set; } = new();
        public List<ProductSizeEditViewModel> Sizes { get; set; } = new List<ProductSizeEditViewModel>();
        public List<IFormFile>? NewImages { get; set; }
        public List<int> ImagesToDelete { get; set; } = new List<int>();
    }

    public class ProductSizeEditViewModel
    {
        public int Id { get; set; } // ID của KichThuocSanPham, 0 nếu là mới
        public string Size { get; set; } = string.Empty;
        public string? Color { get; set; } = "";
        public int StockQuantity { get; set; }
        public decimal? RegularPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public IFormFile? ColorImage { get; set; }
        public bool IsStandard { get; set; }
        public bool IsDeleted { get; set; } = false; // Đánh dấu xóa
    }

    public class GalleryImageViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; } = "";
        public string? Color { get; set; }
        public bool IsMain { get; set; }
    }
} 
