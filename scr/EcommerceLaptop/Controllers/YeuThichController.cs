using Ecommerce.Data;
using Ecommerce.Helpers;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    public class YeuThichController : Controller
    {
        private readonly ApplicationDbContext _context;

        public YeuThichController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = int.Parse(userIdStr);

            var query = _context.YeuThichs
                                .Where(favorite => favorite.NguoiDungId == userId)
                                .Include(favorite => favorite.SanPham)
                                    .ThenInclude(sanPham => sanPham!.DanhGias)
                                .OrderByDescending(favorite => favorite.NgayTao)
                                .Select(favorite => favorite.SanPham!);
            
            int pageSize = 12;
            var paginatedFavorites = await PaginatedList<SanPham>.CreateAsync(query, pageNumber, pageSize);

            return View(paginatedFavorites);
        }
        [HttpGet]
        public async Task<IActionResult> CheckStatus(int id) // Product ID
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { isFavorited = false });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var isFavorited = await _context.YeuThichs
                .AnyAsync(f => f.NguoiDungId == userId && f.SanPhamId == id);

            return Json(new { isFavorited });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id) // Product ID
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, redirectTo = Url.Action("Login", "Account") });
            }
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var existing = await _context.YeuThichs
                .FirstOrDefaultAsync(f => f.NguoiDungId == userId && f.SanPhamId == id);
            bool favorited;
            if (existing != null)
            {
                _context.YeuThichs.Remove(existing);
                favorited = false;
            }
            else
            {
                _context.YeuThichs.Add(new YeuThich { NguoiDungId = userId, SanPhamId = id });
                favorited = true;
            }
            await _context.SaveChangesAsync();
            var favoriteCount = await _context.YeuThichs.CountAsync(f => f.NguoiDungId == userId);
            return Json(new { success = true, favorited, favoriteCount });
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { count = 0 });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var count = await _context.YeuThichs.CountAsync(f => f.NguoiDungId == userId);
            return Json(new { count });
        }
    }
} 
