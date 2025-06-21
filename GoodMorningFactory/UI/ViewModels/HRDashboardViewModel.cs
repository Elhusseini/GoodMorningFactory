using GoodMorningFactory.Core.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Threading.Tasks;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel للوحة معلومات الموارد البشرية.
    /// يعتمد على HRDashboardService لجلب البيانات ويقوم بتجهيزها للعرض.
    /// </summary>
    public class HRDashboardViewModel : BaseViewModel
    {
        private readonly IHRDashboardService _hrDashboardService;

        #region Properties for KPI Cards
        private int _totalActiveEmployees;
        public int TotalActiveEmployees
        {
            get => _totalActiveEmployees;
            set { _totalActiveEmployees = value; OnPropertyChanged(); }
        }

        private int _newHiresLast30Days;
        public int NewHiresLast30Days
        {
            get => _newHiresLast30Days;
            set { _newHiresLast30Days = value; OnPropertyChanged(); }
        }

        private int _terminationsLast30Days;
        public int TerminationsLast30Days
        {
            get => _terminationsLast30Days;
            set { _terminationsLast30Days = value; OnPropertyChanged(); }
        }

        private int _pendingLeaveRequests;
        public int PendingLeaveRequests
        {
            get => _pendingLeaveRequests;
            set { _pendingLeaveRequests = value; OnPropertyChanged(); }
        }
        #endregion

        #region Properties for Charts
        private SeriesCollection _departmentDistribution;
        public SeriesCollection DepartmentDistribution
        {
            get => _departmentDistribution;
            set { _departmentDistribution = value; OnPropertyChanged(); }
        }
        #endregion

        public HRDashboardViewModel()
        {
            _hrDashboardService = new HRDashboardService();
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var kpisTask = _hrDashboardService.GetHRKpisAsync();
                var departmentTask = _hrDashboardService.GetDepartmentDistributionAsync();

                await Task.WhenAll(kpisTask, departmentTask);

                // تحديث مؤشرات الأداء
                var kpis = await kpisTask;
                TotalActiveEmployees = kpis.TotalActiveEmployees;
                NewHiresLast30Days = kpis.NewHiresLast30Days;
                TerminationsLast30Days = kpis.TerminationsLast30Days;
                PendingLeaveRequests = kpis.PendingLeaveRequests;

                // تحديث الرسم البياني
                var departmentData = await departmentTask;
                DepartmentDistribution = new SeriesCollection();
                foreach (var dept in departmentData)
                {
                    DepartmentDistribution.Add(new PieSeries
                    {
                        Title = dept.Key,
                        Values = new ChartValues<int> { dept.Value },
                        DataLabels = true
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"حدث خطأ أثناء تحميل بيانات لوحة المعلومات: {ex.Message}", "خطأ");
            }
        }
    }
}
