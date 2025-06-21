using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العمليات المتعلقة بجلب بيانات لوحة التحكم الرئيسية.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// جلب إجمالي المبيعات لليوم الحالي.
        /// </summary>
        Task<decimal> GetTotalSalesTodayAsync();

        /// <summary>
        /// جلب إجمالي المبيعات للشهر الحالي.
        /// </summary>
        Task<decimal> GetTotalSalesThisMonthAsync();

        /// <summary>
        /// جلب العدد الإجمالي للمنتجات.
        /// </summary>
        Task<int> GetTotalProductsAsync();

        /// <summary>
        /// جلب عدد المنتجات التي وصلت إلى حد إعادة الطلب.
        /// </summary>
        Task<int> GetLowStockProductsCountAsync();

        /// <summary>
        /// جلب بيانات المبيعات لآخر 6 أشهر للرسم البياني.
        /// </summary>
        Task<Dictionary<string, decimal>> GetMonthlySalesDataAsync();

        /// <summary>
        /// جلب قائمة المنتجات الأكثر مبيعًا.
        /// </summary>
        Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(int count);
    }

    /// <summary>
    /// كائن لنقل بيانات المنتجات الأكثر مبيعًا من الخدمة إلى الـ ViewModel.
    /// </summary>
    public class TopSellingProductDto
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
    }
}
