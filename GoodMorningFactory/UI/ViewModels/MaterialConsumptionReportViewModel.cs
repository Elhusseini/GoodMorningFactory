// UI/ViewModels/MaterialConsumptionReportViewModel.cs
// *** ملف جديد: ViewModel خاص ببيانات تقرير تحليل استهلاك المواد ***
namespace GoodMorningFactory.UI.ViewModels
{
    public class MaterialConsumptionReportViewModel
    {
        public string MaterialName { get; set; }
        public decimal PlannedQuantity { get; set; } // الكمية المخطط استهلاكها
        public decimal ActualQuantity { get; set; } // الكمية المستهلكة فعلياً
        public decimal Variance => ActualQuantity - PlannedQuantity; // الفرق (الهدر أو الوفر)
    }
}