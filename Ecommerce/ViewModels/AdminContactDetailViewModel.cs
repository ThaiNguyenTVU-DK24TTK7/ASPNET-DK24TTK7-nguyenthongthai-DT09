using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class AdminContactDetailViewModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ChuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string? PhanHoiAdmin { get; set; }
        public DateTime NgayGui { get; set; }
        public string? TenNguoiDung { get; set; }
        public string? SoDienThoaiNguoiDung { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập phản hồi")]
        [StringLength(1000, ErrorMessage = "Phản hồi không được quá 1000 ký tự")]
        public string? PhanHoiMoi { get; set; }
    }
} 
