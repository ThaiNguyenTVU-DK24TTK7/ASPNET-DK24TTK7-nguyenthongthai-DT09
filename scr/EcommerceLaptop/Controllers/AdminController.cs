using Microsoft.AspNetCore.Mvc;
using Ecommerce.Data;
using Ecommerce.Helpers;
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
using System.Security.Claims;

namespace Ecommerce.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var now = DateTime.Now;
            var salesChart = Enumerable.Range(0, 6).Select(i => {
                var month = now.AddMonths(-5 + i);
                var label = month.ToString("MMM").ToUpper();
                var value = _context.DonHangs
                    .Where(d => d.NgayTao.Month == month.Month && d.NgayTao.Year == month.Year && d.TrangThai == "Đã giao")
                    .Sum(d => (decimal?)d.TongGiaTri) ?? 0;
                return new SalesChartPoint { Label = label, Value = value };
            }).ToList();
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

        [HttpGet]
        public IActionResult GetSalesChartData(string period = "month")
        {
            var now = DateTime.Now;
            List<object> chartData = new List<object>();

            switch (period.ToLower())
            {
                case "week":
                    for (int i = 6; i >= 0; i--)
                    {
                        var date = now.AddDays(-i);
                        var label = date.ToString("ddd", new System.Globalization.CultureInfo("vi-VN"));
                        var value = _context.DonHangs
                            .Where(d => d.NgayTao.Date == date.Date && d.TrangThai == "Đã giao")
                            .Sum(d => (decimal?)d.TongGiaTri) ?? 0;
                        chartData.Add(new { Label = label, Value = value });
                    }
                    break;

                case "year":
                    for (int i = 4; i >= 0; i--)
                    {
                        var year = now.AddYears(-i).Year;
                        var label = year.ToString();
                        var value = _context.DonHangs
                            .Where(d => d.NgayTao.Year == year && d.TrangThai == "Đã giao")
                            .Sum(d => (decimal?)d.TongGiaTri) ?? 0;
                        chartData.Add(new { Label = label, Value = value });
                    }
                    break;

                default: // month
                    for (int i = 5; i >= 0; i--)
                    {
                        var month = now.AddMonths(-i);
                        var label = month.ToString("MMM", new System.Globalization.CultureInfo("vi-VN"));
                        var value = _context.DonHangs
                            .Where(d => d.NgayTao.Month == month.Month && d.NgayTao.Year == month.Year && d.TrangThai == "Đã giao")
                            .Sum(d => (decimal?)d.TongGiaTri) ?? 0;
                        chartData.Add(new { Label = label, Value = value });
                    }
                    break;
            }

            return Json(new
            {
                labels = chartData.Select(x => ((dynamic)x).Label).ToArray(),
                values = chartData.Select(x => ((dynamic)x).Value).ToArray()
            });
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
                .Include(d => d.ThanhToans)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.KichThuocSanPham)
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
                PaymentMethod = order.ThanhToans
                    .OrderByDescending(t => t.NgayTao)
                    .Select(t => t.PhuongThuc)
                    .FirstOrDefault() ?? "Chưa cập nhật",
                TotalAmount = order.TongGiaTri,
                CreatedAt = order.NgayTao,
                Products = order.ChiTietDonHangs.Select(ct => new EditOrderProductViewModel
                {
                    ProductName = ct.SanPham != null ? ct.SanPham.Ten : "",
                    Variant = ct.KichThuocSanPham != null ? ct.KichThuocSanPham.KichThuoc : "",
                    Color = ct.KichThuocSanPham != null ? ct.KichThuocSanPham.MauSac : "",
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

        [HttpGet]
        public async Task<IActionResult> BannerList()
        {
            var model = new AdminBannerViewModel
            {
                Banners = await _context.Banners.OrderBy(b => b.DisplayOrder).ThenBy(b => b.Id).ToListAsync(),
                DisplayOrder = (await _context.Banners.Select(b => (int?)b.DisplayOrder).MaxAsync() ?? -1) + 1
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBanner(AdminBannerViewModel model)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (model.ImageFile == null || model.ImageFile.Length == 0)
                ModelState.AddModelError(nameof(model.ImageFile), "Vui lòng chọn ảnh banner.");
            else if (!allowedExtensions.Contains(Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant()))
                ModelState.AddModelError(nameof(model.ImageFile), "Chỉ hỗ trợ ảnh JPG, PNG hoặc WEBP.");

            if (!ModelState.IsValid)
            {
                model.Banners = await _context.Banners.OrderBy(b => b.DisplayOrder).ThenBy(b => b.Id).ToListAsync();
                return View("BannerList", model);
            }

            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "banners");
            Directory.CreateDirectory(uploadDirectory);
            var fileName = $"banner_{Guid.NewGuid():N}{Path.GetExtension(model.ImageFile!.FileName).ToLowerInvariant()}";
            await using (var stream = new FileStream(Path.Combine(uploadDirectory, fileName), FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            _context.Banners.Add(new Banner
            {
                ImageUrl = $"images/banners/{fileName}", Title = model.Title, LinkUrl = model.LinkUrl,
                DisplayOrder = model.DisplayOrder, IsActive = model.IsActive
            });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm banner thành công.";
            return RedirectToAction(nameof(BannerList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();
            banner.IsActive = !banner.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(BannerList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", banner.ImageUrl.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa banner.";
            return RedirectToAction(nameof(BannerList));
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
                CategoryId = product.DanhMucId,
                Material = product.ChatLieu ?? "",
                Brand = product.ThuongHieu ?? "",
                RegularPrice = product.Gia,
                SalePrice = product.GiaGiam,
                Tags = new List<string>(), // nếu có tags
                MainImageUrl = mainImage != null ? "/" + mainImage.DuongDan : null,
                MainImageId = mainImage?.Id,
                GalleryImages = product.SanPhamAnhs
                    .Where(a => string.IsNullOrWhiteSpace(a.MauSac))
                    .Select(a => new GalleryImageViewModel
                {
                    Id = a.Id,
                    Url = "/" + a.DuongDan,
                    Color = a.MauSac,
                    IsMain = a.IsMain
                }).ToList(),
                Sizes = product.KichThuocSanPhams.Select(k => new ProductSizeEditViewModel
                {
                    Id = k.Id,
                    Size = k.KichThuoc,
                    Color = k.MauSac,
                    StockQuantity = k.TonKho,
                    RegularPrice = k.Gia,
                    SalePrice = k.GiaGiam,
                    IsStandard = k.IsTieuChuan
                }).ToList()
            };
            
            ViewBag.Categories = _context.DanhMucs.ToList();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, AdminEditProductViewModel model, List<IFormFile> newImages)
        {
            ValidateVariantPrices(model.Sizes);
            if (ModelState.IsValid)
            {
                var product = _context.SanPhams
                    .Include(p => p.SanPhamAnhs)
                    .Include(p => p.KichThuocSanPhams)
                    .FirstOrDefault(p => p.Id == id);
                if (product == null) return NotFound();
                product.Ten = model.ProductName;
                product.MoTa = model.Description;
                product.DanhMucId = model.CategoryId;
                product.ChatLieu = model.Material;
                product.ThuongHieu = model.Brand;
                product.Gia = model.RegularPrice;
                product.GiaGiam = model.SalePrice;
                if (model.Sizes != null)
                {
                    var sizesToDelete = model.Sizes.Where(s => s.IsDeleted && s.Id > 0).ToList();
                    foreach (var sizeToDelete in sizesToDelete)
                    {
                        var existingSize = _context.KichThuocSanPhams.FirstOrDefault(k => k.Id == sizeToDelete.Id);
                        if (existingSize != null)
                        {
                            _context.KichThuocSanPhams.Remove(existingSize);
                        }
                    }
                    var sizesToProcess = model.Sizes.Where(s => !s.IsDeleted).ToList();
                    foreach (var sizeModel in sizesToProcess)
                    {
                        if (sizeModel.Id > 0)
                        {
                            var existingSize = _context.KichThuocSanPhams.FirstOrDefault(k => k.Id == sizeModel.Id);
                            if (existingSize != null)
                            {
                                existingSize.KichThuoc = sizeModel.Size;
                                existingSize.MauSac = sizeModel.Color ?? "";
                                existingSize.TonKho = sizeModel.StockQuantity;
                                existingSize.Gia = sizeModel.RegularPrice;
                                existingSize.GiaGiam = sizeModel.SalePrice;
                                existingSize.IsTieuChuan = sizeModel.IsStandard;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(sizeModel.Size))
                            {
                                var newSize = new KichThuocSanPham
                                {
                                    SanPhamId = id,
                                    KichThuoc = sizeModel.Size,
                                    MauSac = sizeModel.Color ?? "",
                                    TonKho = sizeModel.StockQuantity,
                                    Gia = sizeModel.RegularPrice,
                                    GiaGiam = sizeModel.SalePrice,
                                    IsTieuChuan = sizeModel.IsStandard
                                };
                                _context.KichThuocSanPhams.Add(newSize);
                            }
                        }
                    }
                }
                if (newImages != null && newImages.Count > 0)
                {
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
                var colorImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(colorImagePath))
                {
                    Directory.CreateDirectory(colorImagePath);
                }

                foreach (var sizeModel in model.Sizes ?? new List<ProductSizeEditViewModel>())
                {
                    if (sizeModel.IsDeleted || string.IsNullOrWhiteSpace(sizeModel.Color) || sizeModel.ColorImage == null || sizeModel.ColorImage.Length == 0)
                    {
                        continue;
                    }

                    foreach (var existingImage in product.SanPhamAnhs.Where(image =>
                        string.Equals(image.MauSac, sizeModel.Color.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        existingImage.MauSac = null;
                    }

                    var fileName = $"sp_{id}_{Guid.NewGuid():N}{Path.GetExtension(sizeModel.ColorImage.FileName)}";
                    var filePath = Path.Combine(colorImagePath, fileName);
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await sizeModel.ColorImage.CopyToAsync(stream);
                    }

                    _context.SanPhamAnhs.Add(new SanPhamAnh
                    {
                        SanPhamId = id,
                        DuongDan = $"images/{fileName}",
                        MauSac = sizeModel.Color.Trim(),
                        IsMain = false
                    });
                }
                
                await _context.SaveChangesAsync();

                await UpdateProductRepresentativePrice(id);
                var mainImage = await _context.SanPhamAnhs.Where(a => a.SanPhamId == id && a.IsMain).FirstOrDefaultAsync();
                if (mainImage == null)
                {
                    mainImage = await _context.SanPhamAnhs.Where(a => a.SanPhamId == id).OrderBy(a => a.Id).FirstOrDefaultAsync();
                }
                product.DuongDanAnh = mainImage?.DuongDan;
                
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";
                return RedirectToAction("EditProduct", new { id = id });
            }
            ViewBag.Categories = _context.DanhMucs.ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetMainImage([FromBody] SetMainImageRequest request)
        {
            var currentMainImages = _context.SanPhamAnhs.Where(a => a.SanPhamId == request.ProductId && a.IsMain);
            foreach (var img in currentMainImages)
            {
                img.IsMain = false;
            }
            var newMainImage = _context.SanPhamAnhs.FirstOrDefault(a => a.Id == request.ImageId && a.SanPhamId == request.ProductId);
            if (newMainImage != null)
            {
                newMainImage.IsMain = true;
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
            var relatedFavorites = _context.YeuThichs.Where(f => f.SanPhamId == id);
            _context.YeuThichs.RemoveRange(relatedFavorites);

            var relatedReviews = _context.DanhGias.Where(r => r.SanPhamId == id);
            _context.DanhGias.RemoveRange(relatedReviews);

            _context.KichThuocSanPhams.RemoveRange(product.KichThuocSanPhams);
            _context.SanPhamAnhs.RemoveRange(product.SanPhamAnhs);
            _context.SanPhams.Remove(product);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
            return RedirectToAction("ProductList");
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewBag.Categories = _context.DanhMucs.ToList();
            var model = new AdminAddProductViewModel();
            model.SizeTemplate = "custom";
            model.Sizes = new List<ProductSizeViewModel>();
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AdminAddProductViewModel model)
        {
            ValidateVariantPrices(model.Sizes);
            if (ModelState.IsValid)
            {
                var newProduct = new SanPham
                {
                    Ten = model.ProductName,
                    MoTa = model.Description,
                    Gia = model.RegularPrice,
                    GiaGiam = model.SalePrice,
                    ChatLieu = model.Material,
                    ThuongHieu = model.Brand,
                    DanhMucId = model.CategoryId,
                    NgayTao = DateTime.UtcNow
                };
                _context.SanPhams.Add(newProduct);
                await _context.SaveChangesAsync(); // Lưu để lấy Product ID
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
                            var fileName = $"sp_{newProduct.Id}_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(file.FileName)}";
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            
                            var relativePath = $"images/{fileName}";
                            if(firstImagePath == null)
                            {
                                firstImagePath = relativePath;
                            }
                            var newImage = new SanPhamAnh
                            {
                                SanPhamId = newProduct.Id,
                                DuongDan = relativePath,
                                IsMain = firstImagePath == null // Ảnh đầu tiên sẽ là ảnh chính
                            };
                            _context.SanPhamAnhs.Add(newImage);
                        }
                    }
                    if(firstImagePath != null)
                    {
                        newProduct.DuongDanAnh = firstImagePath;
                    }
                }
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
                                TonKho = size.StockQuantity,
                                Gia = size.RegularPrice,
                                GiaGiam = size.SalePrice,
                                IsTieuChuan = size.IsStandard
                            };
                            _context.KichThuocSanPhams.Add(kichThuoc);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await UpdateProductRepresentativePrice(newProduct.Id);
                return RedirectToAction("ProductList");
            }
            ViewBag.Categories = _context.DanhMucs.ToList();
            return View(model);
        }

        private async Task UpdateProductRepresentativePrice(int productId)
        {
            var product = await _context.SanPhams
                .Include(p => p.KichThuocSanPhams)
                .FirstAsync(p => p.Id == productId);
            var variant = ProductPriceHelper.GetDisplayVariant(product);

            if (variant == null)
            {
                return;
            }

            var regularPrice = ProductPriceHelper.GetRegularPrice(product, variant);
            var salePrice = ProductPriceHelper.GetSalePrice(product, variant);
            product.Gia = regularPrice;
            product.GiaGiam = salePrice.HasValue && salePrice.Value < regularPrice
                ? salePrice.Value
                : null;

            await _context.SaveChangesAsync();
        }

        private void ValidateVariantPrices(IEnumerable<ProductSizeEditViewModel>? sizes)
        {
            if (sizes == null)
            {
                return;
            }

            foreach (var size in sizes)
            {
                if (size.RegularPrice.HasValue && size.SalePrice.HasValue && size.SalePrice.Value >= size.RegularPrice.Value)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Giá giảm của phiên bản {size.Size} phải nhỏ hơn giá gốc.");
                }
            }
        }

        private void ValidateVariantPrices(IEnumerable<ProductSizeViewModel>? sizes)
        {
            if (sizes == null)
            {
                return;
            }

            foreach (var size in sizes)
            {
                if (size.RegularPrice.HasValue && size.SalePrice.HasValue && size.SalePrice.Value >= size.RegularPrice.Value)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Giá giảm của phiên bản {size.Size} phải nhỏ hơn giá gốc.");
                }
            }
        }
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
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var uploadPath = Path.Combine(wwwroot, "images");
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.NguoiDungs.Include(u => u.DonHangs).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(UserList));
            }
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
        [Authorize(Roles = "Admin")]
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> PhieuNhapList()
        {
            var phieuNhaps = await _context.PhieuNhaps
                .Include(p => p.NguoiTao)
                .Include(p => p.ChiTietPhieuNhaps)
                .OrderByDescending(p => p.NgayTao)
                .Select(p => new PhieuNhapListViewModel
                {
                    Id = p.Id,
                    MaPhieuNhap = p.MaPhieuNhap,
                    NgayNhap = p.NgayNhap,
                    NhaCungCap = p.NhaCungCap,
                    TongGiaTri = p.TongGiaTri,
                    TrangThai = p.TrangThai,
                    NguoiTao = p.NguoiTao != null ? p.NguoiTao.HoTen : "N/A",
                    TongSanPham = p.ChiTietPhieuNhaps.Sum(ct => ct.SoLuongNhap)
                })
                .ToListAsync();

            return View(phieuNhaps);
        }

        public async Task<IActionResult> AddPhieuNhap()
        {
            ViewBag.Categories = await _context.DanhMucs.ToListAsync();
            
            var model = new AddPhieuNhapViewModel
            {
                MaPhieuNhap = "PN" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                NgayNhap = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhieuNhap(AddPhieuNhapViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hasValidItems = false;
                if (model.ChiTietPhieuNhaps != null && model.ChiTietPhieuNhaps.Any())
                {
                    foreach (var chiTiet in model.ChiTietPhieuNhaps)
                    {
                        if (chiTiet.Sizes != null && chiTiet.Sizes.Any())
                        {
                            hasValidItems = true;
                            break;
                        }
                    }
                }

                if (!hasValidItems)
                {
                    ModelState.AddModelError("", "Phải có ít nhất một sản phẩm với kích thước trong phiếu nhập");
                    ViewBag.Categories = await _context.DanhMucs.ToListAsync();
                    return View(model);
                }
                var existingPhieuNhap = await _context.PhieuNhaps
                    .FirstOrDefaultAsync(p => p.MaPhieuNhap == model.MaPhieuNhap);
                if (existingPhieuNhap != null)
                {
                    ModelState.AddModelError("MaPhieuNhap", "Mã phiếu nhập đã tồn tại");
                    ViewBag.Categories = await _context.DanhMucs.ToListAsync();
                    return View(model);
                }
                decimal tongGiaTri = 0;
                if (model.ChiTietPhieuNhaps != null)
                {
                    foreach (var chiTiet in model.ChiTietPhieuNhaps)
                    {
                        if (chiTiet.Sizes != null)
                        {
                            tongGiaTri += chiTiet.Sizes.Sum(s => s.ThanhTien);
                        }
                    }
                }
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    ModelState.AddModelError("", "Không thể xác định người dùng hiện tại");
                    ViewBag.Categories = await _context.DanhMucs.ToListAsync();
                    return View(model);
                }
                var phieuNhap = new PhieuNhap
                {
                    MaPhieuNhap = model.MaPhieuNhap,
                    NgayNhap = model.NgayNhap,
                    NhaCungCap = model.NhaCungCap,
                    GhiChu = model.GhiChu,
                    TongGiaTri = tongGiaTri,
                    TrangThai = "Hoàn thành",
                    NguoiTaoId = currentUserId,
                    NgayTao = DateTime.Now
                };

                _context.PhieuNhaps.Add(phieuNhap);
                await _context.SaveChangesAsync();
                if (model.ChiTietPhieuNhaps != null)
                {
                    foreach (var chiTiet in model.ChiTietPhieuNhaps)
                    {
                        if (chiTiet.Sizes != null && chiTiet.Sizes.Any())
                        {
                            foreach (var size in chiTiet.Sizes)
                            {
                                var chiTietPhieuNhap = new ChiTietPhieuNhap
                                {
                                    PhieuNhapId = phieuNhap.Id,
                                    SanPhamId = chiTiet.SanPhamId,
                                    KichThuoc = size.KichThuoc,
                                    MauSac = size.MauSac,
                                    SoLuongNhap = size.SoLuongNhap,
                                    GiaNhap = size.GiaNhap,
                                    ThanhTien = size.ThanhTien,
                                    GhiChu = size.GhiChu
                                };
                                _context.ChiTietPhieuNhaps.Add(chiTietPhieuNhap);
                                var targetSize = (size.KichThuoc ?? string.Empty).Trim().ToLower();
                                var targetColor = (size.MauSac ?? string.Empty).Trim().ToLower();

                                var kichThuocSanPham = await _context.KichThuocSanPhams
                                    .FirstOrDefaultAsync(k =>
                                        k.SanPhamId == chiTiet.SanPhamId &&
                                        ((k.KichThuoc ?? string.Empty).Trim().ToLower()) == targetSize &&
                                        ((k.MauSac ?? string.Empty).Trim().ToLower()) == targetColor
                                    );

                                if (kichThuocSanPham != null)
                                {
                                    kichThuocSanPham.TonKho += size.SoLuongNhap;
                                }
                                else
                                {
                                    var normalizedKichThuoc = string.IsNullOrWhiteSpace(size.KichThuoc) ? null : size.KichThuoc!.Trim();
                                    var normalizedMauSac = string.IsNullOrWhiteSpace(size.MauSac) ? null : size.MauSac!.Trim();
                                    var newKichThuoc = new KichThuocSanPham
                                    {
                                        SanPhamId = chiTiet.SanPhamId,
                                        KichThuoc = normalizedKichThuoc,
                                        MauSac = normalizedMauSac,
                                        TonKho = size.SoLuongNhap
                                    };
                                    _context.KichThuocSanPhams.Add(newKichThuoc);
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm phiếu nhập thành công!";
                return RedirectToAction(nameof(PhieuNhapList));
            }

            ViewBag.Categories = await _context.DanhMucs.ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> PhieuNhapDetail(int id)
        {
            var phieuNhap = await _context.PhieuNhaps
                .Include(p => p.NguoiTao)
                .Include(p => p.ChiTietPhieuNhaps)
                    .ThenInclude(ct => ct.SanPham)
                        .ThenInclude(sp => sp.SanPhamAnhs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieuNhap == null)
            {
                return NotFound();
            }

            var model = new PhieuNhapDetailViewModel
            {
                Id = phieuNhap.Id,
                MaPhieuNhap = phieuNhap.MaPhieuNhap,
                NgayNhap = phieuNhap.NgayNhap,
                NhaCungCap = phieuNhap.NhaCungCap,
                GhiChu = phieuNhap.GhiChu,
                TongGiaTri = phieuNhap.TongGiaTri,
                TrangThai = phieuNhap.TrangThai,
                NguoiTao = phieuNhap.NguoiTao?.HoTen ?? "N/A",
                NgayTao = phieuNhap.NgayTao,
                ChiTietPhieuNhaps = phieuNhap.ChiTietPhieuNhaps.Select(ct => new ChiTietPhieuNhapDetailViewModel
                {
                    Id = ct.Id,
                    TenSanPham = ct.SanPham?.Ten ?? "N/A",
                    KichThuoc = ct.KichThuoc,
                    MauSac = ct.MauSac,
                    SoLuongNhap = ct.SoLuongNhap,
                    GiaNhap = ct.GiaNhap,
                    ThanhTien = ct.ThanhTien,
                    GhiChu = ct.GhiChu,
                    AnhSanPham = ct.SanPham?.SanPhamAnhs.Any() == true ? 
                        ct.SanPham.SanPhamAnhs.FirstOrDefault(a => a.IsMain)?.DuongDan ?? 
                        ct.SanPham.SanPhamAnhs.First().DuongDan : 
                        "test1.png"
                }).ToList()
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _context.SanPhams
                .Where(sp => sp.DanhMucId == categoryId)
                .Include(sp => sp.KichThuocSanPhams)
                .Include(sp => sp.SanPhamAnhs)
                .Select(sp => new ProductByCategoryViewModel
                {
                    Id = sp.Id,
                    Ten = sp.Ten,
                    AnhDaiDien = sp.SanPhamAnhs.Any() ? 
                        (sp.SanPhamAnhs.FirstOrDefault(a => a.IsMain) != null ? 
                         sp.SanPhamAnhs.FirstOrDefault(a => a.IsMain)!.DuongDan : 
                         sp.SanPhamAnhs.First().DuongDan) : 
                        "test1.png",
                    Gia = sp.GiaGiam ?? sp.Gia,
                    Sizes = sp.KichThuocSanPhams.Select(k => new ProductSizeStockViewModel
                    {
                        KichThuocId = k.Id,
                        KichThuoc = k.KichThuoc,
                        MauSac = k.MauSac,
                        TonKhoHienTai = k.TonKho
                    }).ToList()
                })
                .ToListAsync();

            return Json(products);
        }
    }

    public class SetMainImageRequest
    {
        public int ProductId { get; set; }
        public int ImageId { get; set; }
    }
}
