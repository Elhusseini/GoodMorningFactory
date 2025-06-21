// UI/ViewModels/ProductionCostReportViewModel.cs
// *** تحديث: تمت إضافة حقول تكلفة العمالة والتكلفة الإجمالية ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ProductionCostReportViewModel
    {
        public string WorkOrderNumber { get; set; }
        public string ProductName { get; set; }
        public int ProducedQuantity { get; set; }
        public DateTime CompletionDate { get; set; }
        public decimal TotalMaterialCost { get; set; }

        // --- بداية التحديث ---
        public decimal TotalLaborCost { get; set; } // تكلفة العمالة
        public decimal TotalCost => TotalMaterialCost + TotalLaborCost; // التكلفة الإجمالية
        // --- نهاية التحديث ---
    }
}
