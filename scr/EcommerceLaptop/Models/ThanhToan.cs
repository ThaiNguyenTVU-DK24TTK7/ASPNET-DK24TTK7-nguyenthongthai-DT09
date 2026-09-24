using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class ThanhToan
    {
        [Key]
        public int Id { get; set; }

        public int? DonHangId { get; set; }
        [ForeignKey("DonHangId")]
        public virtual DonHang? DonHang { get; set; }

        [Required]
        [StringLength(50)]
        public string PhuongThuc { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoTien { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
} 
