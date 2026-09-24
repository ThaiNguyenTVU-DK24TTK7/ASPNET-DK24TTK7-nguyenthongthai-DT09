using Ecommerce.Models;
using System.Linq;

namespace Ecommerce.Helpers
{
    public static class ProductPriceHelper
    {
        public static KichThuocSanPham? GetDisplayVariant(SanPham product)
        {
            var variants = product.KichThuocSanPhams?.ToList() ?? new List<KichThuocSanPham>();
            return variants.OrderBy(v => GetEffectivePrice(product, v)).FirstOrDefault();
        }

        public static decimal GetRegularPrice(SanPham product, KichThuocSanPham? variant = null)
        {
            return variant?.Gia ?? product.Gia;
        }

        public static decimal? GetSalePrice(SanPham product, KichThuocSanPham? variant = null)
        {
            return variant?.GiaGiam ?? product.GiaGiam;
        }

        public static decimal GetEffectivePrice(SanPham product, KichThuocSanPham? variant = null)
        {
            var regular = GetRegularPrice(product, variant);
            var sale = GetSalePrice(product, variant);
            return sale.HasValue && sale.Value < regular ? sale.Value : regular;
        }

    }
}