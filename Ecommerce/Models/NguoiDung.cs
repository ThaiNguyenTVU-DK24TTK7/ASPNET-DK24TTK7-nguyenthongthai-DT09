using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class NguoiDung
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TenDangNhap { get; set; }

        [Required]
        [StringLength(255)]
        public string MatKhau { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Required]
        [StringLength(50)]
        public string VaiTro { get; set; } = "KhachHang";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public virtual ICollection<LienHe>? LienHes { get; set; }
        public virtual ICollection<YeuThich> YeuThichs { get; set; } = new List<YeuThich>();
        public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
    }
} 
