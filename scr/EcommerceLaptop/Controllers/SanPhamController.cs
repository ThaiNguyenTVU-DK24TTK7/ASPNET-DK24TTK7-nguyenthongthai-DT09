using Ecommerce.Data;
using Ecommerce.Helpers;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SanPhamController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string searchString, 
            List<int> danhMucIds, 
            List<string> priceRanges, 
            List<string> productFilters,
            List<string> brands,
            int pageNumber = 1)
        {
            Console.WriteLine($"Fetching products. Filters: Categories:{danhMucIds?.Count ?? 0}, Price ranges:{priceRanges?.Count ?? 0}");
            var query = _context.SanPhams
                .Include(sp => sp.DanhGias)
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.KichThuocSanPhams)
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(p => p.Ten.Contains(searchString));

            if (danhMucIds != null && danhMucIds.Any())
                query = query.Where(p => danhMucIds.Contains(p.DanhMucId));

            if (priceRanges != null && priceRanges.Any())
            {
                var pricePredicate = PredicateBuilder.New<SanPham>();
                foreach (var range in priceRanges)
                {
                    var values = range.Split('-').Select(decimal.Parse).ToList();
                    pricePredicate = pricePredicate.Or(p => (p.GiaGiam ?? p.Gia) >= values[0] && (p.GiaGiam ?? p.Gia) <= values[1]);
                }
                query = query.Where(pricePredicate);
            }

            if (brands != null && brands.Any())
            {
                var brandPredicate = PredicateBuilder.New<SanPham>();
                foreach (var brand in brands)
                {
                    brandPredicate = brandPredicate.Or(p => p.ThuongHieu.ToLower().Contains(brand.ToLower()));
                }
                query = query.Where(brandPredicate);
            }
            var totalCount = await query.CountAsync();
            Console.WriteLine($"Total products found before pagination: {totalCount}");

            int pageSize = 9;
            var paginatedSanPhams = await PaginatedList<SanPham>.CreateAsync(query, pageNumber, pageSize);
            var categoriesWithCount = await _context.DanhMucs
                .Select(dm => new SanPhamPageViewModel.CategoryWithCount
                {
                    Id = dm.Id,
                    Ten = dm.Ten,
                    Count = _context.SanPhams.Count(sp => sp.DanhMucId == dm.Id)
                }).ToListAsync();

            var viewModel = new SanPhamPageViewModel
            {
                SanPhams = paginatedSanPhams,
                AllDanhMucs = await _context.DanhMucs.ToListAsync(),
                CategoriesWithCount = categoriesWithCount,
                SearchString = searchString,
                SelectedDanhMucIds = danhMucIds ?? new List<int>(),
                SelectedPriceRanges = priceRanges ?? new List<string>(),
                SelectedProductFilters = productFilters ?? new List<string>(),
                SelectedBrands = brands ?? new List<string>(),
                PageIndex = pageNumber
            };

            viewModel.AvailableBrands = await _context.SanPhams
                .Where(p => !string.IsNullOrWhiteSpace(p.ThuongHieu))
                .Select(p => p.ThuongHieu)
                .Distinct()
                .OrderBy(brand => brand)
                .ToListAsync();

            return View(viewModel);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .Include(s => s.SanPhamAnhs)
                .Include(s => s.KichThuocSanPhams)
                .Include(s => s.DanhGias)!
                    .ThenInclude(d => d.NguoiDung)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sanPham == null)
            {
                return NotFound();
            }
            var colors = sanPham.KichThuocSanPhams?
                .Where(k => !string.IsNullOrWhiteSpace(k.MauSac))
                .Select(k => k.MauSac.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();
            var sizes = sanPham.KichThuocSanPhams?.Select(k => k.KichThuoc).Distinct().ToList() ?? new List<string>();
            var similarProducts = await _context.SanPhams
                .Include(s => s.DanhGias)
                .Where(s => s.DanhMucId == sanPham.DanhMucId && s.Id != sanPham.Id)
                .OrderByDescending(s => s.SoLuongBan)
                .Take(4)
                .ToListAsync();

            ViewBag.Colors = colors;
            ViewBag.Sizes = sizes;
            ViewBag.SimilarProducts = similarProducts;

            return View(sanPham);
        }
        public async Task<IActionResult> Debug()
        {
            var products = await _context.SanPhams.ToListAsync();
            return Json(new { 
                productCount = products.Count,
                firstFewProducts = products.Take(5).Select(p => new { p.Id, p.Ten, p.Gia })
            });
        }
    }
}
