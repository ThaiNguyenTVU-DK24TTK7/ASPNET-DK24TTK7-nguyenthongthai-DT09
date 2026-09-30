using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn.")]
        public string NoiDung { get; set; }
    }
}
