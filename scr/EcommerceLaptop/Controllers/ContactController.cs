using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.NguoiDungs.FindAsync(userId);

                if (user != null)
                {
                    var contactMessage = new LienHe
                    {
                        HoTen = user.HoTen,
                        Email = user.Email,
                        ChuDe = "Tin nhắn từ khách hàng",
                        NoiDung = model.NoiDung,
                        NgayGui = System.DateTime.UtcNow,
                        NguoiDungId = userId,
                        TrangThai = "Mới"
                    };
                    _context.LienHes.Add(contactMessage);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tin nhắn của bạn đã được gửi thành công!";
                    return RedirectToAction("Index");
                }
            }
            TempData["ErrorMessage"] = "Nội dung tin nhắn không được để trống.";
            return RedirectToAction("Index");
        }
    }
}
