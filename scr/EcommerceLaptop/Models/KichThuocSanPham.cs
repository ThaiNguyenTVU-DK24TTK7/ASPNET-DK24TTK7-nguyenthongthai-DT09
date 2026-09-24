using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class KichThuocSanPham
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }

        [Required]
        [StringLength(10)]
        public string KichThuoc { get; set; }

        [Required]
        [StringLength(50)]
        public string MauSac { get; set; }

        [Required]
        public int TonKho { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Gia { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? GiaGiam { get; set; }

        public bool IsTieuChuan { get; set; }

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
        public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
    }
} 
