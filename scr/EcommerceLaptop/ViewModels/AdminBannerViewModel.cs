using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Ecommerce.Models;

namespace Ecommerce.ViewModels
{
    public class AdminBannerViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ảnh banner")]
        public IFormFile? ImageFile { get; set; }
        [StringLength(150)] public string? Title { get; set; }
        [StringLength(500)] public string? LinkUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Banner> Banners { get; set; } = new();
    }
}