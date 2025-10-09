using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class ChiTietDonHang
    {
        [Key]
        public int Id { get; set; }

        public int? DonHangId { get; set; }
        [ForeignKey("DonHangId")]
        public virtual DonHang? DonHang { get; set; }

        public int? SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }
        
        [Required]
        public int KichThuocSanPhamId { get; set; }
        [ForeignKey("KichThuocSanPhamId")]
        public virtual KichThuocSanPham? KichThuocSanPham { get; set; }

        [Required]
        public int SoLuong { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GiaMua { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
} 
