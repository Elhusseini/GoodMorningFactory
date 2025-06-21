// UI/ViewModels/InventoryDashboardViewModel.cs
// *** تحديث: تمت إضافة خاصية منسقة لعرض قيمة المخزون بالعملة الافتراضية ***
using GoodMorningFactory.Core.Services;
using LiveCharts;

namespace GoodMorningFactory.UI.ViewModels
{
    public class InventoryDashboardViewModel
    {
        // مؤشرات الأداء الرئيسية
        public decimal TotalInventoryValue { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }

        // --- بداية التحديث ---
        public string TotalInventoryValueFormatted => $"{TotalInventoryValue:N2} {AppSettings.DefaultCurrencySymbol}";
        // --- نهاية التحديث ---

        // بيانات الرسم البياني
        public SeriesCollection ValueByCategorySeries { get; set; }
    }
}
