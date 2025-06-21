// GoodMorningFactory/Core/Services/IInventoryDashboardService.cs
using LiveCharts;
using System.Collections.Generic; // <-- إضافة مهمة
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العقد الخاص بخدمة لوحة معلومات المخزون.
    /// </summary>
    public interface IInventoryDashboardService
    {
        /// <summary>
        /// جلب كل البيانات المجمعة اللازمة لعرض لوحة معلومات المخزون.
        /// </summary>
        /// <returns>كائن يحتوي على كل مؤشرات الأداء الرئيسية.</returns>
        Task<InventoryDashboardDataDto> GetDashboardDataAsync();

        // --- بداية الإضافة: تعريف الدوال الجديدة ---

        /// <summary>
        /// جلب قائمة بالمنتجات الأكثر ركوداً (الأقل حركة).
        /// </summary>
        /// <param name="count">عدد المنتجات المراد جلبها.</param>
        /// <param name="stagnantDays">عدد الأيام التي يعتبر المنتج بعدها راكداً.</param>
        /// <returns>قائمة من كائنات DTO تحتوي على معلومات المنتجات الراكدة.</returns>
        Task<List<StagnantProductDto>> GetTopStagnantProductsAsync(int count, int stagnantDays = 90);

        /// <summary>
        /// جلب قائمة بالمنتجات الأعلى قيمة في المخزون.
        /// </summary>
        /// <param name="count">عدد المنتجات المراد جلبها.</param>
        /// <returns>قائمة من كائنات DTO تحتوي على معلومات المنتجات الأعلى قيمة.</returns>
        Task<List<TopValuedProductDto>> GetTopValuedProductsAsync(int count);

        // --- نهاية الإضافة ---
    }
}