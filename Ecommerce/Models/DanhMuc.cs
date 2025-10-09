using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class DanhMuc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Ten { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public string? DuongDanAnh { get; set; }

        // Navigation property
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
} 
