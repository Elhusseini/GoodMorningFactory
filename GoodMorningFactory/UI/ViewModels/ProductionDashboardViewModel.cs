// GoodMorningFactory/UI/ViewModels/ProductionDashboardViewModel.cs
// *** الكود الكامل والنهائي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ProductionDashboardViewModel : BaseViewModel
    {
        private readonly IProductionDashboardService _dashboardService;

        #region Properties
        private int _openWorkOrders;
        public int OpenWorkOrders { get => _openWorkOrders; set { _openWorkOrders = value; OnPropertyChanged(); } }

        private int _completedToday;
        public int CompletedToday { get => _completedToday; set { _completedToday = value; OnPropertyChanged(); } }

        private string _onTimeCompletionRate;
        public string OnTimeCompletionRate { get => _onTimeCompletionRate; set { _onTimeCompletionRate = value; OnPropertyChanged(); } }

        private int _urgentWorkOrders;
        public int UrgentWorkOrders { get => _urgentWorkOrders; set { _urgentWorkOrders = value; OnPropertyChanged(); } }

        public ObservableCollection<WorkOrder> UrgentWorkOrdersList { get; } = new ObservableCollection<WorkOrder>();

        public SeriesCollection WorkOrderStatusSeries { get; set; } = new SeriesCollection();
        #endregion

        public ProductionDashboardViewModel()
        {
            _dashboardService = new ProductionDashboardService();
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                var data = await _dashboardService.GetDashboardDataAsync();

                OpenWorkOrders = data.OpenWorkOrders;
                CompletedToday = data.CompletedToday;
                OnTimeCompletionRate = data.OnTimeCompletionRate;
                UrgentWorkOrders = data.UrgentWorkOrders;

                UrgentWorkOrdersList.Clear();
                foreach (var wo in data.UrgentWorkOrdersList)
                {
                    UrgentWorkOrdersList.Add(wo);
                }

                WorkOrderStatusSeries.Clear();
                foreach (var statusCount in data.StatusCounts)
                {
                    WorkOrderStatusSeries.Add(new PieSeries
                    {
                        Title = statusCount.Key,
                        Values = new ChartValues<int> { statusCount.Value },
                        DataLabels = true
                    });
                }
                OnPropertyChanged(nameof(WorkOrderStatusSeries));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل بيانات لوحة المعلومات: {ex.Message}", "خطأ");
            }
        }
    }
}