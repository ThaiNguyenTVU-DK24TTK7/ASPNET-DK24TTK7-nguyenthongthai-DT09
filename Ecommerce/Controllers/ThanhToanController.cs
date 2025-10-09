using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Globalization;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class ThanhToanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThanhToanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Challenge();
            }

            var user = await _context.NguoiDungs.FindAsync(userId);
            var cartItems = await _context.GioHangs
                                          .Include(g => g.KichThuocSanPham)
                                              .ThenInclude(kt => kt.SanPham) // Ensure SanPham is loaded
                                          .Where(g => g.NguoiDungId == userId)
                                          .ToListAsync();

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "GioHang");
            }

            var subTotal = cartItems.Sum(item => GetProductPrice(item.KichThuocSanPham.SanPham) * item.SoLuong);
            var total = subTotal; // No shipping or tax

            var viewModel = new ThanhToanViewModel
            {
                CartItems = cartItems,
                SubTotal = subTotal,
                ShippingFee = 0,
                Tax = 0,
                Total = total,
                HoTen = user?.HoTen,
                SoDienThoai = user?.SoDienThoai,
                Email = user?.Email,
                NewOrderId = $"#DH{DateTime.Now.Ticks}"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(ThanhToanViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var cartItems = await _context.GioHangs
                                          .Include(g => g.KichThuocSanPham)
                                              .ThenInclude(kt => kt.SanPham)
                                          .Where(g => g.NguoiDungId == userId)
                                          .ToListAsync();

                    if (!cartItems.Any())
                    {
                        return Json(new { success = false, message = "Giỏ hàng trống." });
                    }

                    var subTotal = cartItems.Sum(item => GetProductPrice(item.KichThuocSanPham.SanPham) * item.SoLuong);
                    var total = subTotal; // No shipping or tax

                    var order = new DonHang
                    {
                        NguoiDungId = userId,
                        TenNguoiNhan = model.HoTen,
                        SoDienThoaiNhan = model.SoDienThoai,
                        DiaChiNhan = model.DiaChiGiaoHang,
                        NgayTao = DateTime.UtcNow,
                        TrangThai = "Chờ xử lý",
                        TongGiaTri = total,
                        GhiChu = model.GhiChu
                    };

                    _context.DonHangs.Add(order);
                    await _context.SaveChangesAsync();

                    foreach (var item in cartItems)
                    {
                        var orderDetail = new ChiTietDonHang
                        {
                            DonHangId = order.Id,
                            SanPhamId = item.KichThuocSanPham.SanPhamId,
                            KichThuocSanPhamId = item.KichThuocSanPhamId,
                            SoLuong = item.SoLuong,
                            GiaMua = GetProductPrice(item.KichThuocSanPham.SanPham)
                        };
                        _context.ChiTietDonHangs.Add(orderDetail);
                    }

                    _context.GioHangs.RemoveRange(cartItems);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    
                    var errorMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMessage += " ---> " + ex.InnerException.Message;
                    }

                    // In production, log this error. For debugging, we return it.
                    return Json(new { success = false, message = $"Lỗi máy chủ nội bộ: {errorMessage}" });
                }
            }
        }

        private decimal GetProductPrice(SanPham sanPham)
        {
            return sanPham.GiaGiam.HasValue && sanPham.GiaGiam < sanPham.Gia
                   ? sanPham.GiaGiam.Value
                   : sanPham.Gia;
        }
    }
}
