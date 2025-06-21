// UI/ViewModels/HRDashboardViewModel.cs
// *** ملف جديد: ViewModel خاص ببيانات لوحة معلومات الموارد البشرية ***
using LiveCharts;

namespace GoodMorningFactory.UI.ViewModels
{
    public class HRDashboardViewModel
    {
        public int TotalActiveEmployees { get; set; }
        public int NewHiresLast30Days { get; set; }
        public int TerminationsLast30Days { get; set; }
        public int PendingLeaveRequests { get; set; }
        public SeriesCollection DepartmentDistribution { get; set; }
    }
}