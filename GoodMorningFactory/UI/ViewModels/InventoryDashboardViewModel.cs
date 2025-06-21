// UI/ViewModels/InventoryDashboardViewModel.cs
// *** ملف جديد: ViewModel خاص ببيانات لوحة معلومات المخزون ***
using LiveCharts;

namespace GoodMorningFactory.UI.ViewModels
{
    public class InventoryDashboardViewModel
    {
        // مؤشرات الأداء الرئيسية
        public decimal TotalInventoryValue { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }

        // بيانات الرسم البياني
        public SeriesCollection ValueByCategorySeries { get; set; }
    }
}