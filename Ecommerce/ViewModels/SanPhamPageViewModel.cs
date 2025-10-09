using Ecommerce.Helpers;
using Ecommerce.Models;
using System.Collections.Generic;

namespace Ecommerce.ViewModels
{
    public class SanPhamPageViewModel
    {
        public PaginatedList<SanPham>? SanPhams { get; set; }
        public List<DanhMuc> AllDanhMucs { get; set; } = new();
        public List<CategoryWithCount> CategoriesWithCount { get; set; } = new();
        public string? SearchString { get; set; }
        public int? CategoryId { get; set; }
        public List<int> SelectedDanhMucIds { get; set; } = new();
        public List<string> SelectedPriceRanges { get; set; } = new();
        public List<int> SelectedRatings { get; set; } = new();
        public List<string> SelectedProductFilters { get; set; } = new();
        public List<string> SelectedBrands { get; set; } = new();
        public int PageIndex { get; set; } = 1;

        public class CategoryWithCount
        {
            public int Id { get; set; }
            public string Ten { get; set; } = string.Empty;
            public int Count { get; set; }
        }
    }
} 
