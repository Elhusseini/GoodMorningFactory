// UI/ViewModels/ProductionDashboardViewModel.cs
// *** الكود الكامل لـ ViewModel الخاص ببيانات لوحة معلومات الإنتاج ***
using GoodMorningFactory.Data.Models;
using LiveCharts;
using System.Collections.Generic;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ProductionDashboardViewModel
    {
        public int OpenWorkOrders { get; set; }
        public int CompletedToday { get; set; }
        public string OnTimeCompletionRate { get; set; }
        public int UrgentWorkOrders { get; set; }
        public SeriesCollection WorkOrderStatusSeries { get; set; }
        public List<WorkOrder> UrgentWorkOrdersList { get; set; }

        public ProductionDashboardViewModel()
        {
            UrgentWorkOrdersList = new List<WorkOrder>();
        }
    }
}