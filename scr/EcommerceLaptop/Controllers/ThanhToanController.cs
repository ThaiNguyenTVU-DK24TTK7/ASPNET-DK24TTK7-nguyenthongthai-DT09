using Ecommerce.Data;
using Ecommerce.Helpers;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Ecommerce.Services;
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
            private static readonly TimeSpan MoMoPaymentTimeout = TimeSpan.FromMinutes(5);
        private readonly ApplicationDbContext _context;
        private readonly MoMoService _momoService;

        public ThanhToanController(ApplicationDbContext context, MoMoService momoService)
        {
            _context = context;
            _momoService = momoService;
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

            var subTotal = cartItems.Sum(item => GetProductPrice(item.KichThuocSanPham) * item.SoLuong);
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

                    var subTotal = cartItems.Sum(item => GetProductPrice(item.KichThuocSanPham) * item.SoLuong);
                    var total = subTotal; // No shipping or tax

                    var order = new DonHang
                    {
                        NguoiDungId = userId,
                        TenNguoiNhan = model.HoTen,
                        SoDienThoaiNhan = model.SoDienThoai,
                        DiaChiNhan = model.DiaChiGiaoHang,
                        NgayTao = DateTime.UtcNow,
                        TrangThai = model.PhuongThucThanhToan == "MoMo QR" ? "Chờ thanh toán" : "Chờ xử lý",
                        TongGiaTri = total,
                        GhiChu = model.GhiChu
                    };

                    _context.DonHangs.Add(order);
                    await _context.SaveChangesAsync();

                    _context.ThanhToans.Add(new ThanhToan
                    {
                        DonHangId = order.Id,
                        PhuongThuc = model.PhuongThucThanhToan,
                        SoTien = total,
                        TrangThai = model.PhuongThucThanhToan == "MoMo QR"
                            ? "Chờ thanh toán"
                            : "Chưa thanh toán",
                        NgayTao = DateTime.UtcNow
                    });

                    foreach (var item in cartItems)
                    {
                        var orderDetail = new ChiTietDonHang
                        {
                            DonHangId = order.Id,
                            SanPhamId = item.KichThuocSanPham.SanPhamId,
                            KichThuocSanPhamId = item.KichThuocSanPhamId,
                            SoLuong = item.SoLuong,
                            GiaMua = GetProductPrice(item.KichThuocSanPham)
                        };
                        _context.ChiTietDonHangs.Add(orderDetail);
                    }

                    _context.GioHangs.RemoveRange(cartItems);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    if (model.PhuongThucThanhToan == "MoMo QR")
                    {
                        var momoTestUrl = await GenerateMoMoTestUrl(order.Id, total, model.HoTen);
                        return Json(new { success = true, redirectUrl = momoTestUrl, paymentMethod = "MoMo QR" });
                    }
                    
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
                    return Json(new { success = false, message = $"Lỗi máy chủ nội bộ: {errorMessage}" });
                }
            }
        }

        private decimal GetProductPrice(KichThuocSanPham variant)
        {
            return ProductPriceHelper.GetEffectivePrice(variant.SanPham, variant);
        }

        private async Task<string> GenerateMoMoTestUrl(int orderId, decimal amount, string customerName)
        {
            try
            {
                var request = new MoMoCreatePaymentRequest
                {
                    OrderId = orderId,
                    Amount = amount,
                    CustomerName = customerName
                };

                var result = await _momoService.CreatePaymentAsync(request);
                
                if (result.Success && !string.IsNullOrEmpty(result.PayUrl))
                {
                    return result.PayUrl; // Chuyển hướng đến cổng thanh toán MoMo thật
                }
                else
                {
                    return Url.Action("PaymentError", "ThanhToan", new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Url.Action("PaymentError", "ThanhToan", new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ContinuePayment(int orderId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Challenge();
            }

            var order = await _context.DonHangs
                .FirstOrDefaultAsync(o => o.Id == orderId && o.NguoiDungId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.TrangThai != "Chờ thanh toán")
            {
                TempData["PaymentError"] = "Đơn hàng này không còn chờ thanh toán.";
                return RedirectToAction("OrderHistory", "Account");
            }

            if (await ExpireMoMoOrderIfNeeded(order))
            {
                TempData["PaymentError"] = "Đơn hàng đã hết thời gian chờ thanh toán và đã được hủy.";
                return RedirectToAction("OrderHistory", "Account");
            }

            var paymentUrl = await GenerateMoMoTestUrl(order.Id, order.TongGiaTri, order.TenNguoiNhan);
            return Redirect(paymentUrl);
        }

        [HttpGet]
        public IActionResult MoMoPayment(int orderId, decimal amount, string customerName)
        {
            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
            ViewBag.RawAmount = amount;
            ViewBag.CustomerName = customerName;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMoMoPayment([FromBody] MoMoCreatePaymentRequest request)
        {
            try
            {
                var result = await _momoService.CreatePaymentAsync(request);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckMoMoPaymentStatus(string orderId, string requestId)
        {
            try
            {
                if (TryGetOrderId(orderId, out var realOrderId))
                {
                    var order = await _context.DonHangs.FindAsync(realOrderId);
                    if (order != null && await ExpireMoMoOrderIfNeeded(order))
                    {
                        return Json(new { resultCode = "1000", message = "Đơn hàng đã hết thời gian thanh toán." });
                    }
                }

                var result = await _momoService.QueryPaymentAsync(orderId, requestId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { resultCode = "1001", message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentReturn(string partnerCode, string orderId, string requestId, string amount, string orderInfo, string orderType, string transId, string resultCode, string message, string payType, string responseTime, string extraData, string signature)
        {
            try
            {
                var logInfo = $"MoMo Callback - OrderId: {orderId}, ResultCode: {resultCode}, Message: {message}";
                Console.WriteLine(logInfo);
                if (resultCode == "0")
                {
                    if (!string.IsNullOrEmpty(orderId) && orderId.StartsWith("DH"))
                    {
                        if (TryGetOrderId(orderId, out int realOrderId))
                        {
                            var order = await _context.DonHangs.FindAsync(realOrderId);
                            if (order != null)
                            {
                                if (await ExpireMoMoOrderIfNeeded(order))
                                {
                                    TempData["PaymentError"] = "Đơn hàng đã hết thời gian chờ thanh toán và đã được hủy.";
                                    return RedirectToAction("Index", "GioHang");
                                }
                                order.TrangThai = "Chờ xử lý";
                                var payment = await _context.ThanhToans
                                    .Where(t => t.DonHangId == order.Id)
                                    .OrderByDescending(t => t.NgayTao)
                                    .FirstOrDefaultAsync();
                                if (payment != null)
                                {
                                    payment.TrangThai = "Đã thanh toán";
                                }
                                await _context.SaveChangesAsync();
                                return View("PaymentSuccess", new PaymentSuccessViewModel 
                                { 
                                    OrderId = realOrderId.ToString(),
                                    Amount = (Convert.ToDecimal(amount) / 100).ToString("N0", new CultureInfo("vi-VN")),
                                    TransactionId = transId,
                                    PaymentMethod = "MoMo QR"
                                });
                            }
                        }
                    }
                    return View("PaymentSuccess", new PaymentSuccessViewModel 
                    { 
                        OrderId = "N/A",
                        Amount = (Convert.ToDecimal(amount ?? "0") / 100).ToString("N0", new CultureInfo("vi-VN")),
                        TransactionId = transId,
                        PaymentMethod = "MoMo QR"
                    });
                }
                else
                {
                    TempData["PaymentError"] = $"Thanh toán thất bại: {message ?? "Giao dịch không thành công"}";
                    return RedirectToAction("Index", "GioHang");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PaymentReturn: {ex.Message}");
                TempData["PaymentError"] = "Có lỗi xảy ra khi xử lý kết quả thanh toán.";
                return RedirectToAction("Index", "GioHang");
            }
        }

        [HttpPost]
        public IActionResult PaymentNotify()
        {
            return Ok();
        }

        [HttpGet]
        public IActionResult PaymentError(string message)
        {
            ViewBag.ErrorMessage = message ?? "Có lỗi xảy ra trong quá trình thanh toán.";
            return View();
        }

        private static bool TryGetOrderId(string momoOrderId, out int orderId)
        {
            orderId = 0;
            if (string.IsNullOrWhiteSpace(momoOrderId) || !momoOrderId.StartsWith("DH"))
            {
                return false;
            }

            var firstPart = momoOrderId.Split('_')[0];
            return int.TryParse(firstPart[2..], out orderId);
        }

        private async Task<bool> ExpireMoMoOrderIfNeeded(DonHang order)
        {
            if (order.TrangThai != "Chờ thanh toán" || order.NgayTao > DateTime.UtcNow - MoMoPaymentTimeout)
            {
                return order.TrangThai == "Đã hủy";
            }

            order.TrangThai = "Đã hủy";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
