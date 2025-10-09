using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class DanhGiaController : Controller
    {
        private readonly Data.ApplicationDbContext _context;
        public DanhGiaController(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(int ProductId, int OrderId, int SoSao, string NoiDung)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (_context.DanhGias.Any(dg => dg.NguoiDungId == userId && dg.SanPhamId == ProductId && dg.DonHangId == OrderId))
                return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này trong đơn này!" });

            var review = new DanhGia
            {
                NguoiDungId = userId,
                SanPhamId = ProductId,
                DonHangId = OrderId,
                SoSao = SoSao,
                BinhLuan = NoiDung,
                TrangThai = "Hiển thị",
                NgayTao = DateTime.Now
            };
            _context.DanhGias.Add(review);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
} 
