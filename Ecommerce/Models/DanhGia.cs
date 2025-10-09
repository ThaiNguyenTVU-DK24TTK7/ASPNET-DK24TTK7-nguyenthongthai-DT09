using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class DanhGia
    {
        [Key]
        public int Id { get; set; }

        public int? NguoiDungId { get; set; }
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        public int? SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }

        public int? SoSao { get; set; }

        [StringLength(1000)]
        public string? BinhLuan { get; set; }

        [Required]
        [StringLength(20)]
        public string TrangThai { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? DonHangId { get; set; }
        [ForeignKey("DonHangId")]
        public virtual DonHang? DonHang { get; set; }
    }
} 
