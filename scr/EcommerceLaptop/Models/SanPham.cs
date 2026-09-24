using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class SanPham
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Ten { get; set; }

        [Required]
        public string MoTa { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Gia { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? GiaGiam { get; set; }

        [Required]
        [StringLength(50)]
        public string ChatLieu { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ThuongHieu { get; set; } = string.Empty;

        [StringLength(500)]
        public string? DuongDanAnh { get; set; }

        [Required]
        public int DanhMucId { get; set; }
        [ForeignKey("DanhMucId")]
        public virtual DanhMuc? DanhMuc { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int SoLuongBan { get; set; } = 0;

        public virtual ICollection<KichThuocSanPham> KichThuocSanPhams { get; set; } = new List<KichThuocSanPham>();
        public virtual ICollection<SanPhamAnh> SanPhamAnhs { get; set; } = new List<SanPhamAnh>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public virtual ICollection<YeuThich> YeuThichs { get; set; } = new List<YeuThich>();
        public virtual ICollection<ChiTietDonHang>? ChiTietDonHangs { get; set; }
    }
} 
