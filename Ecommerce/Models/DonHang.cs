using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class DonHang
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string TenNguoiNhan { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SoDienThoaiNhan { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string DiaChiNhan { get; set; } = string.Empty;

        public string? GhiChu { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Chờ xác nhận";

        public int? NguoiDungId { get; set; }
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongGiaTri { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
        public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
    }
} 
