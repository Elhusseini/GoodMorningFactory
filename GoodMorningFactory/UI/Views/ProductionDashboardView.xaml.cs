// UI/Views/ProductionDashboardView.xaml.cs
// *** ملف جديد: الكود الخلفي للوحة معلومات الإنتاج ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class ProductionDashboardView : UserControl
    {
        public ProductionDashboardView()
        {
            InitializeComponent();
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // إنشاء كائن ViewModel لتخزين البيانات
                    var viewModel = new ProductionDashboardViewModel();
                    var today = DateTime.Today;

                    // --- حساب مؤشرات الأداء الرئيسية ---
                    var openStatuses = new[] { WorkOrderStatus.Planned, WorkOrderStatus.InProgress, WorkOrderStatus.OnHold };
                    viewModel.OpenWorkOrders = db.WorkOrders.Count(wo => openStatuses.Contains(wo.Status));
                    viewModel.CompletedToday = db.WorkOrders.Count(wo => wo.ActualEndDate.HasValue && wo.ActualEndDate.Value.Date == today);

                    var completedOrders = db.WorkOrders.Where(wo => wo.Status == WorkOrderStatus.Completed && wo.ActualEndDate.HasValue).ToList();
                    int onTimeCount = completedOrders.Count(wo => wo.ActualEndDate.Value.Date <= wo.PlannedEndDate.Date);
                    viewModel.OnTimeCompletionRate = completedOrders.Any() ? $"{(double)onTimeCount / completedOrders.Count:P0}" : "N/A";

                    // --- جلب قائمة بالأوامر العاجلة ---
                    viewModel.UrgentWorkOrdersList = db.WorkOrders.Include(wo => wo.FinishedGood)
                        .Where(wo => openStatuses.Contains(wo.Status) && wo.PlannedEndDate <= today.AddDays(3))
                        .OrderBy(wo => wo.PlannedEndDate).ToList();
                    viewModel.UrgentWorkOrders = viewModel.UrgentWorkOrdersList.Count;


                    // --- إعداد بيانات الرسم البياني لحالة أوامر العمل ---
                    var statusCounts = db.WorkOrders
                        .GroupBy(wo => wo.Status)
                        .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                        .ToList();

                    viewModel.WorkOrderStatusSeries = new SeriesCollection();
                    foreach (var statusCount in statusCounts)
                    {
                        viewModel.WorkOrderStatusSeries.Add(new PieSeries
                        {
                            Title = statusCount.Status,
                            Values = new ChartValues<int> { statusCount.Count },
                            DataLabels = true
                        });
                    }

                    // ربط الـ ViewModel بالواجهة لعرض البيانات
                    this.DataContext = viewModel;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل بيانات لوحة المعلومات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}