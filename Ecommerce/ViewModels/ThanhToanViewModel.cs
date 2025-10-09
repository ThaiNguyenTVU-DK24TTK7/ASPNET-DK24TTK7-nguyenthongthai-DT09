using Ecommerce.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class ThanhToanViewModel
    {
        public NguoiDung NguoiDung { get; set; }
        public List<GioHang> CartItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        public string DiaChiGiaoHang { get; set; }

        public string? GhiChu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string SoDienThoai { get; set; }
        
        public string Email { get; set; }

        [Required]
        public DateTime NgayThanhToan { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; }

        // For display only
        public string NewOrderId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
} 
