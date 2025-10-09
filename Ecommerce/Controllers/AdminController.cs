using Microsoft.AspNetCore.Mvc;
using Ecommerce.Data;
using Ecommerce.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Ecommerce.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // Doanh số theo tháng (6 tháng gần nhất)
            var now = DateTime.Now;
            var salesChart = Enumerable.Range(0, 6).Select(i => {
                var month = now.AddMonths(-5 + i);
                var label = month.ToString("MMM").ToUpper();
                var value = _context.DonHangs
                    .Where(d => d.NgayTao.Month == month.Month && d.NgayTao.Year == month.Year && d.TrangThai == "Đã giao")
                    .Sum(d => (decimal?)d.TongGiaTri) ?? 0;
                return new SalesChartPoint { Label = label, Value = value };
            }).ToList();

            // Sản phẩm bán chạy (top 3)
            var topProducts = _context.SanPhams
                .Include(sp => sp.SanPhamAnhs)
                .OrderByDescending(sp => sp.SoLuongBan)
                .Take(3)
                .Select(sp => new TopProductViewModel
                {
                    ProductId = sp.Id,
                    ProductName = sp.Ten,
                    ImageUrl = sp.SanPhamAnhs.Any() ? 
                        (sp.SanPhamAnhs.FirstOrDefault(a => a.IsMain) != null ? sp.SanPhamAnhs.FirstOrDefault(a => a.IsMain).DuongDan : sp.SanPhamAnhs.First().DuongDan) 
                        : "test1.png",
                    Price = sp.GiaGiam ?? sp.Gia,
                    SoldCount = sp.SoLuongBan
                }).ToList();

            // Đơn hàng gần đây (6 đơn mới nhất)
            var recentOrders = _context.DonHangs
                .OrderByDescending(d => d.NgayTao)
                .Take(6)
                .SelectMany(dh => dh.ChiTietDonHangs.Select(ct => new RecentOrderViewModel
                {
                    ProductName = ct.SanPham != null ? ct.SanPham.Ten : "",
                    OrderId = dh.Id,
                    Date = dh.NgayTao,
                    CustomerName = dh.NguoiDung != null ? dh.NguoiDung.HoTen : dh.TenNguoiNhan,
                    Status = dh.TrangThai,
                    Amount = ct.GiaMua * ct.SoLuong
                }))
                .ToList();

            // Tính toán thống kê tổng quan
            var totalOrders = _context.DonHangs.Count();
            var totalUsers = _context.NguoiDungs.Count();
            var totalProducts = _context.SanPhams.Count();
            var totalRevenue = _context.DonHangs
                .Where(d => d.TrangThai == "Đã giao")
                .Sum(d => (decimal?)d.TongGiaTri) ?? 0;
var totalContacts = _context.LienHes.Count();
            var newContacts = _context.LienHes.Count(c => c.TrangThai == "Mới");

            var vm = new DashboardViewModel
            {
                SalesChart = salesChart,
                TopProducts = topProducts,
                RecentOrders = recentOrders,
                TotalOrders = totalOrders,
                TotalUsers = totalUsers,
                TotalProducts = totalProducts,
                TotalRevenue = totalRevenue,
                TotalContacts = totalContacts,
                NewContacts = newContacts
            };
            return View(vm);
        }

        public IActionResult OrderList()
        {
            var orders = _context.DonHangs
                .Include(d => d.NguoiDung)
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new AdminOrderListViewModel
                {
                    OrderId = d.Id,
                    OrderCode = $"#DH{d.Id:0000}",
                    CustomerName = d.NguoiDung != null ? d.NguoiDung.HoTen : d.TenNguoiNhan,
                    Status = d.TrangThai,
                    TotalAmount = d.TongGiaTri,
                    CreatedAt = d.NgayTao
                })
                .ToList();
            return View(orders);
        }

        [HttpGet]
        public IActionResult EditOrderStatus(int id)
        {
            var order = _context.DonHangs
                .Include(d => d.NguoiDung)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .FirstOrDefault(d => d.Id == id);
            if (order == null) return NotFound();
            var vm = new EditOrderStatusViewModel
            {
                OrderId = order.Id,
                OrderCode = $"#DH{order.Id:0000}",
                CurrentStatus = order.TrangThai,
                CustomerName = order.NguoiDung != null ? order.NguoiDung.HoTen : order.TenNguoiNhan,
                Phone = order.SoDienThoaiNhan,
                Address = order.DiaChiNhan,
                Note = order.GhiChu,
                TotalAmount = order.TongGiaTri,
                CreatedAt = order.NgayTao,
                Products = order.ChiTietDonHangs.Select(ct => new EditOrderProductViewModel
                {
                    ProductName = ct.SanPham != null ? ct.SanPham.Ten : "",
                    Quantity = ct.SoLuong,
                    Price = ct.GiaMua
                }).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditOrderStatus(EditOrderStatusViewModel model)
        {
            var order = _context.DonHangs.FirstOrDefault(d => d.Id == model.OrderId);
            if (order == null) return NotFound();
            order.TrangThai = model.NewStatus;
            _context.SaveChanges();
            return RedirectToAction("OrderList");
}

        public IActionResult ProductList()
        {
            var products = _context.SanPhams
                .Include(p => p.SanPhamAnhs)
                .OrderByDescending(p => p.NgayTao)
                .Select(p => new AdminProductListViewModel
                {
                    ProductId = p.Id,
                    ProductName = p.Ten,
                    Price = p.GiaGiam ?? p.Gia,
                    ImageUrl = p.SanPhamAnhs.Any() ? 
                        "/" + (p.SanPhamAnhs.FirstOrDefault(a => a.IsMain) != null ? p.SanPhamAnhs.FirstOrDefault(a => a.IsMain).DuongDan : p.SanPhamAnhs.First().DuongDan) 
                        : "/images/placeholder.png",
                    SoldCount = p.SoLuongBan,
                    CreatedAt = p.NgayTao
                })
                .ToList();
            return View(products);
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = _context.SanPhams
                .Include(p => p.SanPhamAnhs)
                .Include(p => p.DanhMuc)
                .Include(p => p.KichThuocSanPhams)
                .FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var mainImage = product.SanPhamAnhs.FirstOrDefault(a => a.IsMain) ?? product.SanPhamAnhs.FirstOrDefault();
            
            var vm = new AdminEditProductViewModel
            {
                ProductId = product.Id,
                ProductName = product.Ten,
                Description = product.MoTa,
                Category = product.DanhMuc?.Ten ?? "",
                SKU = product.Id.ToString(),
                StockQuantity = product.KichThuocSanPhams.Sum(k => k.TonKho),
                RegularPrice = product.Gia,
                SalePrice = product.GiaGiam,
                Tags = new List<string>(), // nếu có tags
                MainImageUrl = mainImage != null ? "/" + mainImage.DuongDan : null,
                MainImageId = mainImage?.Id,
                GalleryImages = product.SanPhamAnhs.Select(a => new GalleryImageViewModel
                {
                    Id = a.Id,
                    Url = "/" + a.DuongDan,
                    IsMain = a.IsMain
                }).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, AdminEditProductViewModel model, List<IFormFile> newImages)
        {
            var product = _context.SanPhams.Include(p => p.SanPhamAnhs).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            // Cập nhật thông tin cơ bản
            product.Ten = model.ProductName;
            product.MoTa = model.Description;
            product.Gia = model.RegularPrice;
            product.GiaGiam = model.SalePrice;
            
            // Xử lý upload ảnh mới - THAY THẾ ảnh cũ
            if (newImages != null && newImages.Count > 0)
            {
                // 1. Xóa ảnh cũ
                var oldImages = _context.SanPhamAnhs.Where(i => i.SanPhamId == id).ToList();
                if (oldImages.Any())
                {
                    var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    foreach (var image in oldImages)
                    {
if (!string.IsNullOrEmpty(image.DuongDan))
                        {
                            var imagePath = Path.Combine(wwwrootPath, image.DuongDan.Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(imagePath))
                            {
                                System.IO.File.Delete(imagePath);
                            }
                        }
                    }
                    _context.SanPhamAnhs.RemoveRange(oldImages);
                }

                // 2. Thêm ảnh mới
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                bool isFirstImage = true;
                foreach (var file in newImages)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"sp_{id}_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var newImage = new SanPhamAnh 
                        { 
                            SanPhamId = id, 
                            DuongDan = $"images/{fileName}",
                            IsMain = isFirstImage // Ảnh đầu tiên sẽ là ảnh chính
                        };
                        _context.SanPhamAnhs.Add(newImage);
                        isFirstImage = false;
                    }
                }
            }
            
            await _context.SaveChangesAsync();

            // 3. Cập nhật lại ảnh đại diện cho sản phẩm (ưu tiên ảnh chính)
            var mainImage = await _context.SanPhamAnhs.Where(a => a.SanPhamId == id && a.IsMain).FirstOrDefaultAsync();
            if (mainImage == null)
            {
                // Nếu không có ảnh chính, lấy ảnh đầu tiên
                mainImage = await _context.SanPhamAnhs.Where(a => a.SanPhamId == id).OrderBy(a => a.Id).FirstOrDefaultAsync();
            }
            product.DuongDanAnh = mainImage?.DuongDan;
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";
            return RedirectToAction("EditProduct", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> SetMainImage([FromBody] SetMainImageRequest request)
        {
            // Bỏ đánh dấu ảnh chính cũ
            var currentMainImages = _context.SanPhamAnhs.Where(a => a.SanPhamId == request.ProductId && a.IsMain);
            foreach (var img in currentMainImages)
            {
                img.IsMain = false;
            }

            // Đánh dấu ảnh mới là ảnh chính
            var newMainImage = _context.SanPhamAnhs.FirstOrDefault(a => a.Id == request.ImageId && a.SanPhamId == request.ProductId);
            if (newMainImage != null)
            {
                newMainImage.IsMain = true;
                
                // Cập nhật ảnh đại diện cho sản phẩm
                var product = _context.SanPhams.FirstOrDefault(p => p.Id == request.ProductId);
                if (product != null)
                {
                    product.DuongDanAnh = newMainImage.DuongDan;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã cập nhật ảnh chính thành công!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.SanPhams
                .Include(p => p.ChiTietDonHangs)
                .Include(p => p.SanPhamAnhs)
                .Include(p => p.KichThuocSanPhams)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("ProductList");
            }

            if (product.ChiTietDonHangs != null && product.ChiTietDonHangs.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm đã có trong đơn hàng của khách.";
                return RedirectToAction("ProductList");
            }
// Xóa file ảnh vật lý
            if (product.SanPhamAnhs != null)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                foreach (var image in product.SanPhamAnhs)
                {
                    if (!string.IsNullOrEmpty(image.DuongDan))
                    {
                        var filePath = Path.Combine(uploadPath, image.DuongDan.Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }
            }

            // Xóa các thực thể liên quan
            var relatedFavorites = _context.YeuThichs.Where(f => f.SanPhamId == id);
            _context.YeuThichs.RemoveRange(relatedFavorites);

            var relatedReviews = _context.DanhGias.Where(r => r.SanPhamId == id);
            _context.DanhGias.RemoveRange(relatedReviews);

            _context.KichThuocSanPhams.RemoveRange(product.KichThuocSanPhams);
            _context.SanPhamAnhs.RemoveRange(product.SanPhamAnhs);

            // Xóa sản phẩm
            _context.SanPhams.Remove(product);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
            return RedirectToAction("ProductList");
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewBag.Categories = _context.DanhMucs.ToList();
            return View(new AdminAddProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AdminAddProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Tạo và lưu sản phẩm chính
                var newProduct = new SanPham
                {
                    Ten = model.ProductName,
                    MoTa = model.Description,
                    Gia = model.RegularPrice,
                    GiaGiam = model.SalePrice,
                    ChatLieu = model.Material,
                    DanhMucId = model.CategoryId,
                    NgayTao = DateTime.UtcNow
                };
                _context.SanPhams.Add(newProduct);
                await _context.SaveChangesAsync(); // Lưu để lấy Product ID

                // 2. Xử lý và lưu hình ảnh
                if (model.ProductImages != null && model.ProductImages.Count > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    
                    string firstImagePath = null; // Biến để lưu đường dẫn ảnh đầu tiên
foreach (var file in model.ProductImages)
                    {
                        if (file.Length > 0)
                        {
                            // Tạo tên file duy nhất để tránh trùng lặp
                            var fileName = $"sp_{newProduct.Id}_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(file.FileName)}";
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            
                            var relativePath = $"images/{fileName}";

                            // Nếu đây là ảnh đầu tiên, lưu đường dẫn của nó
                            if(firstImagePath == null)
                            {
                                firstImagePath = relativePath;
                            }

                            // Lưu đường dẫn vào bảng gallery
                            var newImage = new SanPhamAnh
                            {
                                SanPhamId = newProduct.Id,
                                DuongDan = relativePath,
                                IsMain = firstImagePath == null // Ảnh đầu tiên sẽ là ảnh chính
                            };
                            _context.SanPhamAnhs.Add(newImage);
                        }
                    }
                    
                    // Gán ảnh đầu tiên làm ảnh đại diện cho sản phẩm
                    if(firstImagePath != null)
                    {
                        newProduct.DuongDanAnh = firstImagePath;
                    }
                }
                
                // 3. Xử lý và lưu kích thước & tồn kho
                if (model.Sizes != null && model.Sizes.Any())
                {
                    foreach (var size in model.Sizes)
                    {
                        if (size.StockQuantity > 0)
                        {
                            var kichThuoc = new KichThuocSanPham
                            {
                                SanPhamId = newProduct.Id,
                                KichThuoc = size.Size,
                                MauSac = string.IsNullOrWhiteSpace(size.Color) ? "" : size.Color,
                                TonKho = size.StockQuantity
                            };
                            _context.KichThuocSanPhams.Add(kichThuoc);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("ProductList");
            }

            // Nếu model không hợp lệ, load lại danh mục và trả về view
            ViewBag.Categories = _context.DanhMucs.ToList();
            return View(model);
        }

        // Category Management
        public IActionResult CategoryList()
        {
            var categories = _context.DanhMucs
.Include(c => c.SanPhams)
                .Select(c => new AdminCategoryListViewModel
                {
                    Id = c.Id,
                    Ten = c.Ten,
                    ProductCount = c.SanPhams.Count()
                })
                .ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AdminCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = new DanhMuc
                {
                    Ten = model.Ten,
                    MoTa = model.MoTa,
                    NgayTao = DateTime.Now
                };

                // Xử lý upload ảnh nếu có
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var safeFileName = $"cat_{Guid.NewGuid().ToString().Substring(0,8)}{Path.GetExtension(model.ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadPath, safeFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                    category.DuongDanAnh = $"images/{safeFileName}";
                }

                _context.DanhMucs.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Danh mục đã được tạo thành công!";
                return RedirectToAction("CategoryList");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.DanhMucs.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var model = new AdminCategoryViewModel
            {
                Id = category.Id,
                Ten = category.Ten,
                MoTa = category.MoTa
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, AdminCategoryViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _context.DanhMucs.FindAsync(id);
                    if (category == null)
                    {
                        return NotFound();
                    }
                    category.Ten = model.Ten;
                    category.MoTa = model.MoTa;

                    // Nếu có tải ảnh mới -> lưu ảnh và cập nhật đường dẫn
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var uploadPath = Path.Combine(wwwroot, "images");
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        // Xóa ảnh cũ nếu tồn tại
                        if (!string.IsNullOrEmpty(category.DuongDanAnh))
                        {
                            var oldPath = Path.Combine(wwwroot, category.DuongDanAnh.Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        var fileName = $"cat_{Guid.NewGuid().ToString().Substring(0,8)}{Path.GetExtension(model.ImageFile.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }
                        category.DuongDanAnh = $"images/{fileName}";
                    }
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DanhMucs.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Danh mục đã được cập nhật thành công!";
                return RedirectToAction("CategoryList");
            }
            return View(model);
        }

        [HttpPost]
[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.DanhMucs.Include(c => c.SanPhams).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
                return RedirectToAction("CategoryList");
            }

            if (category.SanPhams.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục này vì vẫn còn sản phẩm.";
                return RedirectToAction("CategoryList");
            }

            _context.DanhMucs.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Danh mục đã được xóa thành công!";
            return RedirectToAction("CategoryList");
        }

        // User Management
        public async Task<IActionResult> UserList()
        {
            var users = await _context.NguoiDungs
                .Include(u => u.DonHangs)
                .Select(u => new AdminUserListViewModel
                {
                    Id = u.Id,
                    HoTen = u.HoTen,
                    Email = u.Email,
                    VaiTro = u.VaiTro,
                    NgayTao = u.NgayTao,
                    OrderCount = u.DonHangs.Count()
                })
                .ToListAsync();
            
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AdminEditUserViewModel
            {
                Id = user.Id,
                HoTen = user.HoTen,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                VaiTro = user.VaiTro
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, AdminEditUserViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var user = await _context.NguoiDungs.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.HoTen = model.HoTen;
                user.Email = model.Email;
                user.SoDienThoai = model.SoDienThoai;
                user.VaiTro = model.VaiTro;

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thông tin người dùng đã được cập nhật.";
return RedirectToAction(nameof(UserList));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Không thể lưu thay đổi. Vui lòng thử lại.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.NguoiDungs.Include(u => u.DonHangs).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(UserList));
            }

            // An toàn: Không cho xóa admin khác hoặc người dùng đã có đơn hàng.
            if (user.VaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản Quản trị viên.";
                return RedirectToAction(nameof(UserList));
            }

            if (user.DonHangs.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng đã có đơn hàng.";
                return RedirectToAction(nameof(UserList));
            }

            _context.NguoiDungs.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Người dùng đã được xóa thành công.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(AdminAddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại.");
                }
                if (await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                }

                if (ModelState.ErrorCount == 0)
                {
                    var user = new NguoiDung
                    {
                        TenDangNhap = model.TenDangNhap,
                        HoTen = model.HoTen,
                        Email = model.Email,
                        SoDienThoai = model.SoDienThoai,
                        MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau),
                        VaiTro = model.VaiTro,
                        NgayTao = DateTime.Now
                    };

                    _context.NguoiDungs.Add(user);
                    await _context.SaveChangesAsync();
TempData["SuccessMessage"] = "Tạo người dùng mới thành công!";
                    return RedirectToAction(nameof(UserList));
                }
            }

            return View(model);
        }

        // Contact Management
        public async Task<IActionResult> ContactList()
        {
            var contacts = await _context.LienHes
                .Include(l => l.NguoiDung)
                .OrderByDescending(l => l.NgayGui)
                .Select(l => new AdminContactListViewModel
                {
                    Id = l.Id,
                    HoTen = l.HoTen,
                    Email = l.Email,
                    ChuDe = l.ChuDe,
                    NoiDung = l.NoiDung.Length > 100 ? l.NoiDung.Substring(0, 100) + "..." : l.NoiDung,
                    TrangThai = l.TrangThai,
                    PhanHoiAdmin = l.PhanHoiAdmin,
                    NgayGui = l.NgayGui,
                    TenNguoiDung = l.NguoiDung != null ? l.NguoiDung.HoTen : null
                })
                .ToListAsync();

            return View(contacts);
        }

        [HttpGet]
        public async Task<IActionResult> ContactDetail(int id)
        {
            var contact = await _context.LienHes
                .Include(l => l.NguoiDung)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (contact == null)
            {
                return NotFound();
            }

            var model = new AdminContactDetailViewModel
            {
                Id = contact.Id,
                HoTen = contact.HoTen,
                Email = contact.Email,
                ChuDe = contact.ChuDe,
                NoiDung = contact.NoiDung,
                TrangThai = contact.TrangThai,
                PhanHoiAdmin = contact.PhanHoiAdmin,
                NgayGui = contact.NgayGui,
                TenNguoiDung = contact.NguoiDung?.HoTen,
                SoDienThoaiNguoiDung = contact.NguoiDung?.SoDienThoai,
                PhanHoiMoi = contact.PhanHoiAdmin
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactDetail(int id, AdminContactDetailViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var contact = await _context.LienHes.FindAsync(id);
                if (contact == null)
                {
                    return NotFound();
                }

                contact.PhanHoiAdmin = model.PhanHoiMoi;
                contact.TrangThai = "Đã phản hồi";

                try
                {
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Phản hồi đã được lưu thành công!";
return RedirectToAction(nameof(ContactList));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Không thể lưu phản hồi. Vui lòng thử lại.");
                }
            }

            // Nếu có lỗi, load lại thông tin contact
            var contactReload = await _context.LienHes
                .Include(l => l.NguoiDung)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (contactReload != null)
            {
                model.HoTen = contactReload.HoTen;
                model.Email = contactReload.Email;
                model.ChuDe = contactReload.ChuDe;
                model.NoiDung = contactReload.NoiDung;
                model.TrangThai = contactReload.TrangThai;
                model.PhanHoiAdmin = contactReload.PhanHoiAdmin;
                model.NgayGui = contactReload.NgayGui;
                model.TenNguoiDung = contactReload.NguoiDung?.HoTen;
                model.SoDienThoaiNguoiDung = contactReload.NguoiDung?.SoDienThoai;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _context.LienHes.FindAsync(id);
            if (contact == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin nhắn liên hệ.";
                return RedirectToAction(nameof(ContactList));
            }

            _context.LienHes.Remove(contact);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tin nhắn liên hệ đã được xóa thành công.";
            return RedirectToAction(nameof(ContactList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var contact = await _context.LienHes.FindAsync(id);
            if (contact == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin nhắn liên hệ.";
                return RedirectToAction(nameof(ContactList));
            }

            contact.TrangThai = "Đã đọc";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã đánh dấu tin nhắn là đã đọc.";
            return RedirectToAction(nameof(ContactList));
        }
    }

    public class SetMainImageRequest
    {
        public int ProductId { get; set; }
        public int ImageId { get; set; }
    }
}
