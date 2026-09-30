    using System.ComponentModel.DataAnnotations;

    namespace Ecommerce.ViewModels
    {
        public class ProfileViewModel
        {
            [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
            [StringLength(100)]
            [Display(Name = "Họ và tên")]
            public string HoTen { get; set; }

            [Required(ErrorMessage = "Email là bắt buộc.")]
            [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
            [Display(Name = "Số điện thoại")]
            public string? SoDienThoai { get; set; }
        }
    }
