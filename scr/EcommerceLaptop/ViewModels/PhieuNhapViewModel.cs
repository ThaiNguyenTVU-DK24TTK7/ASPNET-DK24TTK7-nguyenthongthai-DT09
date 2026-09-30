using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class PhieuNhapListViewModel
    {
        public int Id { get; set; }
        public string MaPhieuNhap { get; set; } = string.Empty;
        public DateTime NgayNhap { get; set; }
        public string? NhaCungCap { get; set; }
        public decimal TongGiaTri { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string NguoiTao { get; set; } = string.Empty;
        public int TongSanPham { get; set; }
    }

    public class AddPhieuNhapViewModel
    {
        [Required(ErrorMessage = "Mã phiếu nhập là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã phiếu nhập không được vượt quá 50 ký tự")]
        public string MaPhieuNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày nhập là bắt buộc")]
        public DateTime NgayNhap { get; set; } = DateTime.Now;

        [StringLength(200, ErrorMessage = "Tên nhà cung cấp không được vượt quá 200 ký tự")]
        public string? NhaCungCap { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? GhiChu { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int DanhMucId { get; set; }

        public List<ChiTietPhieuNhapViewModel> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhapViewModel>();
    }

    public class ChiTietPhieuNhapViewModel
    {
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string? KichThuoc { get; set; }
        public string? MauSac { get; set; }
        public int? SoLuongNhap { get; set; }
        public decimal? GiaNhap { get; set; }

        public decimal ThanhTien => (SoLuongNhap ?? 0) * (GiaNhap ?? 0);

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? GhiChu { get; set; }
        public List<PhieuNhapSizeViewModel> Sizes { get; set; } = new List<PhieuNhapSizeViewModel>();
    }

    public class PhieuNhapSizeViewModel
    {
        [Required(ErrorMessage = "Kích thước là bắt buộc")]
        public string? KichThuoc { get; set; }
        public string? MauSac { get; set; }

        [Required(ErrorMessage = "Số lượng nhập là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0")]
        public int SoLuongNhap { get; set; }

        [Required(ErrorMessage = "Giá nhập là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn hoặc bằng 0")]
        public decimal GiaNhap { get; set; }

        public decimal ThanhTien => SoLuongNhap * GiaNhap;

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? GhiChu { get; set; }
    }

    public class PhieuNhapDetailViewModel
    {
        public int Id { get; set; }
        public string MaPhieuNhap { get; set; } = string.Empty;
        public DateTime NgayNhap { get; set; }
        public string? NhaCungCap { get; set; }
        public string? GhiChu { get; set; }
        public decimal TongGiaTri { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string NguoiTao { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }

        public List<ChiTietPhieuNhapDetailViewModel> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhapDetailViewModel>();
    }

    public class ChiTietPhieuNhapDetailViewModel
    {
        public int Id { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string? KichThuoc { get; set; }
        public string? MauSac { get; set; }
        public int SoLuongNhap { get; set; }
        public decimal GiaNhap { get; set; }
        public decimal ThanhTien { get; set; }
        public string? GhiChu { get; set; }
        public string? AnhSanPham { get; set; }
    }

    public class ProductByCategoryViewModel
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public decimal Gia { get; set; }
        public List<ProductSizeStockViewModel> Sizes { get; set; } = new List<ProductSizeStockViewModel>();
    }

    public class ProductSizeStockViewModel
    {
        public int? KichThuocId { get; set; }
        public string? KichThuoc { get; set; }
        public string? MauSac { get; set; }
        public int TonKhoHienTai { get; set; }
    }
}