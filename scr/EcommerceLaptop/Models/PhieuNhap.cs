using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    [Table("PhieuNhaps")]
    public class PhieuNhap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string MaPhieuNhap { get; set; } = string.Empty;

        [Required]
        public DateTime NgayNhap { get; set; }

        [StringLength(200)]
        public string? NhaCungCap { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Required]
        public decimal TongGiaTri { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Đang xử lý"; // Đang xử lý, Hoàn thành, Hủy

        public int NguoiTaoId { get; set; }
        
        [ForeignKey("NguoiTaoId")]
        public virtual NguoiDung? NguoiTao { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();
    }
}