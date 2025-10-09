using Ecommerce.Data;
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

            var subtotal = cartItems.Sum(item => GetProductPrice(item.KichThuocSanPham.SanPham) * item.SoLuong);
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
            if (model == null || !ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var intUserId))
            {
                return Json(new { success = false, message = "Xác thực thất bại, vui lòng đăng nhập lại.", redirectTo = Url.Action("Login", "Account") });
            }
            
            // Find the specific product variant by size.
            // Note: This assumes size is sufficient. If multiple colors exist for the same size, this will pick the first one.
            var kichThuocSanPham = await _context.KichThuocSanPhams.FirstOrDefaultAsync(k =>
                k.SanPhamId == model.SanPhamId &&
                k.KichThuoc == model.Size);

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

            return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng." });
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
                // Cannot return Unauthorized() from a private method returning JsonResult for other actions.
                // Returning an empty/error result. The calling actions are protected by [Authorize] anyway.
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            var cartItems = await _context.GioHangs
                .Where(g => g.NguoiDungId == userId)
                .Include(g => g.KichThuocSanPham.SanPham)
                .ToListAsync();
            
            var subtotal = cartItems.Sum(item => GetProductPrice(item.KichThuocSanPham.SanPham) * item.SoLuong);
            var total = subtotal; // No cart-wide discount

            return Json(new { 
                success = true, 
                total = total.ToString("N0")
            });
        }

        private decimal GetProductPrice(SanPham sanPham)
        {
            return sanPham.GiaGiam.HasValue && sanPham.GiaGiam < sanPham.Gia 
                   ? sanPham.GiaGiam.Value 
                   : sanPham.Gia;
        }
    }

    public class AddToCartModel
    {
        public int SanPhamId { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
    }
} 
