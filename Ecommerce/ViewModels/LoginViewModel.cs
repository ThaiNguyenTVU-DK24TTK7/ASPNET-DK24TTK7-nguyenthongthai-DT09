using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Display(Name = "Ghi nhớ tôi?")]
        public bool GhiNhoToi { get; set; }
    }
} 
