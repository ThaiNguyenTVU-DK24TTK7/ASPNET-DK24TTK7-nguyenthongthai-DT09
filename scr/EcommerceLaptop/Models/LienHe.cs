using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class LienHe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ChuDe { get; set; } = string.Empty;

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string TrangThai { get; set; }

        public string? PhanHoiAdmin { get; set; }

        public DateTime NgayGui { get; set; } = DateTime.Now;

        public int? NguoiDungId { get; set; }
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }
    }
} 
