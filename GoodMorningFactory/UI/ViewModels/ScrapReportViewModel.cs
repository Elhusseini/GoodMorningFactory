// UI/ViewModels/ScrapReportViewModel.cs
// *** ملف جديد: ViewModel خاص ببيانات تقرير الهدر ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ScrapReportViewModel
    {
        public string WorkOrderNumber { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public DateTime Date { get; set; }
    }
}