using Ecommerce.Data;
using Ecommerce.Helpers;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GioHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var intUserId))
            {
                return Unauthorized();
            }

            var cartItems = await _context.GioHangs
                .Where(g => g.NguoiDungId == intUserId)
                .Include(g => g.KichThuocSanPham)
                    .ThenInclude(ktsp => ktsp.SanPham)
                .ToListAsync();

            var subtotal = cartItems.Sum(item => GetProductPrice(item.KichThuocSanPham) * item.SoLuong);
            var total = subtotal; // Total is now the subtotal, no extra discount

            var viewModel = new GioHangViewModel
            {
                CartItems = cartItems,
                Subtotal = subtotal,
                Discount = 0, // No cart-wide discount
                Total = total
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartModel model)
        {
            if (model == null || !ModelState.IsValid || model.Quantity <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var intUserId))
            {
                return Json(new { success = false, message = "Xác thực thất bại, vui lòng đăng nhập lại.", redirectTo = Url.Action("Login", "Account") });
            }
            var kichThuocSanPham = await _context.KichThuocSanPhams.FirstOrDefaultAsync(k =>
                k.SanPhamId == model.SanPhamId &&
                (model.VariantId.HasValue ? k.Id == model.VariantId.Value :
                    (!string.IsNullOrEmpty(model.Size) ? k.KichThuoc == model.Size : k.TonKho > 0)));

            if (kichThuocSanPham == null)
            {
                return Json(new { success = false, message = "Sản phẩm với kích thước này không tồn tại." });
            }

            if (kichThuocSanPham.TonKho < model.Quantity)
            {
                return Json(new { success = false, message = "Sản phẩm không đủ số lượng tồn kho." });
            }

            var gioHangItem = await _context.GioHangs.FirstOrDefaultAsync(g =>
                g.NguoiDungId == intUserId &&
                g.KichThuocSanPhamId == kichThuocSanPham.Id);

            if (gioHangItem != null)
            {
                if (gioHangItem.SoLuong + model.Quantity > kichThuocSanPham.TonKho)
                {
                    return Json(new { success = false, message = "Sản phẩm không đủ số lượng tồn kho." });
                }

                gioHangItem.SoLuong += model.Quantity;
            }
            else
            {
                gioHangItem = new GioHang
                {
                    NguoiDungId = intUserId,
                    KichThuocSanPhamId = kichThuocSanPham.Id,
                    SoLuong = model.Quantity,
                    NgayTao = System.DateTime.Now
                };
                _context.GioHangs.Add(gioHangItem);
            }

            await _context.SaveChangesAsync();

            var cartCount = await _context.GioHangs
                .Where(g => g.NguoiDungId == intUserId)
                .SumAsync(g => g.SoLuong);

            return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng.", cartCount });
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { count = 0 });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var count = await _context.GioHangs
                .Where(g => g.NguoiDungId == userId)
                .SumAsync(g => g.SoLuong);
            return Json(new { count });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveFromCart(cartItemId);
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Json(new { success = false, message = "Xác thực thất bại." });
            }
            var cartItem = await _context.GioHangs.FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.NguoiDungId == userId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }

            cartItem.SoLuong = quantity;
            await _context.SaveChangesAsync();

            return await GetCartSubtotalAsJson();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Json(new { success = false, message = "Xác thực thất bại." });
            }
            var cartItem = await _context.GioHangs.FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.NguoiDungId == userId);
            
            if (cartItem != null)
            {
                _context.GioHangs.Remove(cartItem);
                await _context.SaveChangesAsync();
            } else {
                 return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }

            return await GetCartSubtotalAsJson();
        }

        private async Task<JsonResult> GetCartSubtotalAsJson()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            var cartItems = await _context.GioHangs
                .Where(g => g.NguoiDungId == userId)
                .Include(g => g.KichThuocSanPham.SanPham)
                .ToListAsync();
            
            var subtotal = cartItems.Sum(item => GetProductPrice(item.KichThuocSanPham) * item.SoLuong);
            var total = subtotal; // No cart-wide discount

            return Json(new { 
                success = true, 
                total = total.ToString("N0")
            });
        }

        private decimal GetProductPrice(KichThuocSanPham variant)
        {
            return ProductPriceHelper.GetEffectivePrice(variant.SanPham, variant);
        }
    }

    public class AddToCartModel
    {
        public int SanPhamId { get; set; }
        public int? VariantId { get; set; }
        public string? Size { get; set; }
        public int Quantity { get; set; }
    }
} 
