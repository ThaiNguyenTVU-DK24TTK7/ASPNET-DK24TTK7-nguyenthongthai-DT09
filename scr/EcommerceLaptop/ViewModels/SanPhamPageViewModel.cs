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
    public List<string> AvailableSizes { get; set; } = new();
    public List<string> SelectedSizes { get; set; } = new();
    public List<SizeWithCategory> SizesWithCategories { get; set; } = new();
    public List<CategorySizeGroup> CategorySizeGroups { get; set; } = new();

    public List<string> AvailableColors { get; set; } = new();
    public List<string> SelectedColors { get; set; } = new();

    public List<string> AvailableBrands { get; set; } = new();

        public class CategoryWithCount
        {
            public int Id { get; set; }
            public string Ten { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        public class SizeWithCategory
        {
            public string Size { get; set; } = string.Empty;
            public List<string> Categories { get; set; } = new();
            public bool IsSelected { get; set; }
        }

        public class CategorySizeGroup
        {
            public string CategoryName { get; set; } = string.Empty;
            public List<string> Sizes { get; set; } = new();
            public List<string> SelectedSizes { get; set; } = new();
        }
    }
} 
