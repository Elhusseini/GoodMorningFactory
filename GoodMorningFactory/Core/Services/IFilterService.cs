using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العمليات المتعلقة بجلب بيانات الفلاتر والقوائم المنسدلة.
    /// </summary>
    public interface IFilterService
    {
        Task<List<FilterItem<int>>> GetCategoryFiltersAsync();
        Task<List<FilterItem<int>>> GetSupplierFiltersAsync();
        List<FilterItem<ProductType?>> GetProductTypeFilters();
        List<FilterItem<bool?>> GetStatusFilters();
        CompanyInfo GetCompanyInfo();
        Task<List<PriceList>> GetPriceListsAsync();
        List<FilterItem<OrderStatus?>> GetOrderStatusFilters();

        /// <summary>
        /// دالة جديدة لجلب قائمة حالات الشحن.
        /// </summary>
        List<FilterItem<ShipmentStatus?>> GetShipmentStatusFilters();
    }
}
