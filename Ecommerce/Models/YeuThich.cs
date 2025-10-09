using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class YeuThich
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NguoiDungId { get; set; }
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        [Required]
        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
} 
