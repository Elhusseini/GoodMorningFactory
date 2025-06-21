// UI/ViewModels/ProductionCostReportViewModel.cs
// *** الكود الكامل لـ ViewModel الخاص ببيانات تقرير تكلفة الإنتاج ***
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
    }
}