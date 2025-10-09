using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class GioHang
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NguoiDungId { get; set; }
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        [Required]
        public int KichThuocSanPhamId { get; set; }
        [ForeignKey("KichThuocSanPhamId")]
        public virtual KichThuocSanPham? KichThuocSanPham { get; set; }

        [Required]
        public int SoLuong { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
} 
