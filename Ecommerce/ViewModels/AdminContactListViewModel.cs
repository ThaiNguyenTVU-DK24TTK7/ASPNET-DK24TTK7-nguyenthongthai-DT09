using System;

namespace Ecommerce.ViewModels
{
    public class AdminContactListViewModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ChuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string? PhanHoiAdmin { get; set; }
        public DateTime NgayGui { get; set; }
        public string? TenNguoiDung { get; set; }
        public bool CoPhanHoi => !string.IsNullOrEmpty(PhanHoiAdmin);
    }
} 
