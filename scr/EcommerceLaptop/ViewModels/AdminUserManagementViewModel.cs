using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class AdminUserListViewModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public int OrderCount { get; set; }
    }

    public class AdminEditUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public string VaiTro { get; set; } = string.Empty;
    }

    public class AdminAddUserViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string XacNhanMatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public string VaiTro { get; set; } = "KhachHang";
    }
} 
