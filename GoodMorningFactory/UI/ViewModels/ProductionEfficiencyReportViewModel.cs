// UI/ViewModels/ProductionEfficiencyReportViewModel.cs
// *** ملف جديد: ViewModel خاص ببيانات تقرير كفاءة الإنتاج ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ProductionEfficiencyReportViewModel
    {
        // رقم أمر العمل للرجوع إليه
        public string WorkOrderNumber { get; set; }
        // اسم المنتج الذي تم إنتاجه
        public string ProductName { get; set; }
        // المدة المخططة بالأيام
        public double PlannedDurationDays { get; set; }
        // المدة الفعلية التي استغرقها الإنتاج بالأيام
        public double ActualDurationDays { get; set; }

        // خاصية محسوبة لعرض نسبة الكفاءة بشكل منسق
        public string Efficiency
        {
            get
            {
                // لتجنب القسمة على صفر إذا كانت المدة الفعلية غير محددة
                if (ActualDurationDays == 0) return "N/A";
                // حساب النسبة المئوية
                // إذا كانت المدة الفعلية أقل من المخططة، تكون الكفاءة أكبر من 100%
                return (PlannedDurationDays / ActualDurationDays).ToString("P0");
            }
        }
    }
}