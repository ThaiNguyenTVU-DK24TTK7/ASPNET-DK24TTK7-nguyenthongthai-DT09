using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    [Table("ChiTietPhieuNhaps")]
    public class ChiTietPhieuNhap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PhieuNhapId { get; set; }

        [Required]
        public int SanPhamId { get; set; }

        [StringLength(50)]
        public string? KichThuoc { get; set; }

        [StringLength(50)]
        public string? MauSac { get; set; }

        [Required]
        public int SoLuongNhap { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaNhap { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ThanhTien { get; set; }

        [StringLength(200)]
        public string? GhiChu { get; set; }
        [ForeignKey("PhieuNhapId")]
        public virtual PhieuNhap? PhieuNhap { get; set; }

        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }
    }
}