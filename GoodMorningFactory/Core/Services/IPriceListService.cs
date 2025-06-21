// GoodMorningFactory/Core/Services/IPriceListService.cs
// *** الكود الكامل والشامل لواجهة الخدمة الموحدة ***
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IPriceListService
    {
        /// <summary>
        /// جلب جميع قوائم الأسعار.
        /// </summary>
        Task<List<PriceList>> GetPriceListsAsync();

        /// <summary>
        /// جلب قائمة أسعار واحدة مع أسعار المنتجات المرتبطة بها.
        /// </summary>
        Task<PriceList> GetPriceListByIdAsync(int priceListId);

        /// <summary>
        /// حفظ (إضافة أو تحديث) قائمة أسعار (الاسم والوصف فقط).
        /// </summary>
        Task SavePriceListAsync(PriceList priceList);

        /// <summary>
        /// حذف قائمة أسعار مع كل أسعار المنتجات المرتبطة بها.
        /// </summary>
        Task DeletePriceListAsync(int priceListId);

        /// <summary>
        /// جلب جميع المنتجات المتاحة للإضافة إلى قائمة أسعار.
        /// </summary>
        Task<List<Product>> GetAvailableProductsAsync();

        /// <summary>
        /// حفظ التغييرات على أسعار المنتجات في قائمة معينة.
        /// </summary>
        Task SaveProductPricesAsync(int priceListId, IEnumerable<ProductPrice> productPrices);
    }
}