// GoodMorningFactory/Core/Services/InventoryDashboardDataDto.cs
using LiveCharts;
using System.Collections.Generic; // <-- إضافة مهمة

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كائن بسيط لنقل بيانات لوحة معلومات المخزون من طبقة الخدمات إلى طبقة الواجهة.
    /// </summary>
    public class InventoryDashboardDataDto
    {
        public decimal TotalInventoryValue { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public SeriesCollection ValueByCategorySeries { get; set; }

        // --- بداية الإضافة: قوائم جديدة للبيانات الإضافية ---
        public List<StagnantProductDto> StagnantProducts { get; set; }
        public List<TopValuedProductDto> TopValuedProducts { get; set; }
        // --- نهاية الإضافة ---

        public InventoryDashboardDataDto()
        {
            StagnantProducts = new List<StagnantProductDto>();
            TopValuedProducts = new List<TopValuedProductDto>();
        }
    }

    // --- بداية الإضافة: كائنات DTO جديدة ---
    /// <summary>
    /// DTO لنقل بيانات المنتج الراكد.
    /// </summary>
    public class StagnantProductDto
    {
        public string ProductName { get; set; }
        public int DaysSinceLastMovement { get; set; }
        public int QuantityOnHand { get; set; }
    }

    /// <summary>
    /// DTO لنقل بيانات المنتج الأعلى قيمة.
    /// </summary>
    public class TopValuedProductDto
    {
        public string ProductName { get; set; }
        public decimal TotalValue { get; set; }
        public int QuantityOnHand { get; set; }
    }
    // --- نهاية الإضافة ---
}