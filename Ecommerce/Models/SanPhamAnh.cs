using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class SanPhamAnh
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }

        [Required]
        [StringLength(500)]
        public string DuongDan { get; set; } = string.Empty;

        public bool IsMain { get; set; } = false;
    }
} 
