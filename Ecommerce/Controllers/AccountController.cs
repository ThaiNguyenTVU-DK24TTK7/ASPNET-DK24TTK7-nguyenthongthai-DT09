using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                if (await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                    return View(model);
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);

                var nguoiDung = new NguoiDung
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = hashedPassword, 
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    VaiTro = "KhachHang", 
                    NgayTao = System.DateTime.Now
                };

                _context.Add(nguoiDung);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.MatKhau, user.MatKhau))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.HoTen),
                        new Claim(ClaimTypes.Role, user.VaiTro),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.GhiNhoToi,
                        ExpiresUtc = model.GhiNhoToi ? DateTimeOffset.UtcNow.AddDays(30) : (DateTimeOffset?)null
                    };
                    
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme, 
                        new ClaimsPrincipal(claimsIdentity), 
                        authProperties);

                    if (user.VaiTro == "Admin")
                        return RedirectToAction("Dashboard", "Admin");
                    return RedirectToLocal(returnUrl);
                }
                
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Challenge(); 
            }

            var orders = await _context.DonHangs
                .Where(o => o.NguoiDungId.HasValue && o.NguoiDungId.Value == userId)
                .Include(o => o.ChiTietDonHangs)
                    .ThenInclude(od => od.SanPham)
                        .ThenInclude(p => p.SanPhamAnhs)
                .OrderByDescending(o => o.NgayTao)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.Id,
                    OrderCode = $"#DH{o.Id:0000}",
                    OrderDate = o.NgayTao,
                    TotalAmount = o.TongGiaTri,
                    Status = o.TrangThai,
                    ShippingAddress = o.DiaChiNhan,
                    Items = o.ChiTietDonHangs
                        .Where(od => od.SanPhamId.HasValue && od.SanPham != null)
                        .Select(od => new OrderItemViewModel
                        {
                            ProductId = od.SanPhamId.Value,
                            ProductName = od.SanPham.Ten,
                            ProductImageUrl = od.SanPham.SanPhamAnhs.Any()
                                ? "/" + (od.SanPham.SanPhamAnhs.FirstOrDefault(a => a.IsMain) != null ? od.SanPham.SanPhamAnhs.FirstOrDefault(a => a.IsMain).DuongDan : od.SanPham.SanPhamAnhs.First().DuongDan)
                                : "/images/placeholder.png",
                            IsReviewed = _context.DanhGias.Any(dg => dg.NguoiDungId == userId && dg.SanPhamId == od.SanPhamId && dg.DonHangId == o.Id)
                        }).ToList()
                })
                .ToListAsync();

            return View(orders);
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.NguoiDungs.FindAsync(int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                HoTen = user.HoTen,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.NguoiDungs.FindAsync(int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            if (await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email && u.Id != user.Id))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng bởi một tài khoản khác.");
                return View(model);
            }

            user.HoTen = model.HoTen;
            user.Email = model.Email;
            user.SoDienThoai = model.SoDienThoai;

            _context.Update(user);
            await _context.SaveChangesAsync();

            var identity = (ClaimsIdentity)User.Identity;
            var nameClaim = identity.FindFirst(ClaimTypes.Name);
            if (nameClaim != null && nameClaim.Value != user.HoTen)
            {
                identity.RemoveClaim(nameClaim);
                identity.AddClaim(new Claim(ClaimTypes.Name, user.HoTen));
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            }
            
            ViewData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.MatKhau))
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không đúng.");
                return View(model);
            }

            user.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile"); 
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
} 
