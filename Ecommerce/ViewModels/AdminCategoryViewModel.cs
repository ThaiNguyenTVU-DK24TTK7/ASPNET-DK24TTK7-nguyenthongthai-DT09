using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.ViewModels
{
    public class AdminCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string Ten { get; set; } = string.Empty;

        public string? MoTa { get; set; }
        public string? DuongDanAnh { get; set; } // Đường dẫn ảnh danh mục

        // File ảnh tải lên khi tạo/cập nhật danh mục
        public IFormFile? ImageFile { get; set; }
    }

    public class AdminCategoryListViewModel
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }
} 
