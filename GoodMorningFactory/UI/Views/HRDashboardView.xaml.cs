// UI/Views/HRDashboardView.xaml.cs
// *** ملف جديد: الكود الخلفي للوحة معلومات الموارد البشرية ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class HRDashboardView : UserControl
    {
        public HRDashboardView()
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
                    var viewModel = new HRDashboardViewModel();
                    var thirtyDaysAgo = DateTime.Now.AddDays(-30);

                    // --- حساب مؤشرات الأداء الرئيسية ---
                    viewModel.TotalActiveEmployees = db.Employees.Count(e => e.Status == EmployeeStatus.Active);
                    viewModel.NewHiresLast30Days = db.Employees.Count(e => e.HireDate >= thirtyDaysAgo);
                    viewModel.TerminationsLast30Days = db.Employees.Count(e => e.TerminationDate.HasValue && e.TerminationDate.Value >= thirtyDaysAgo);
                    viewModel.PendingLeaveRequests = db.LeaveRequests.Count(lr => lr.Status == LeaveRequestStatus.Pending);

                    // --- إعداد بيانات الرسم البياني ---
                    var departmentDistribution = db.Employees
                        .Where(e => e.Status == EmployeeStatus.Active && !string.IsNullOrEmpty(e.Department))
                        .GroupBy(e => e.Department)
                        .Select(g => new { DepartmentName = g.Key, Count = g.Count() })
                        .ToList();

                    viewModel.DepartmentDistribution = new SeriesCollection();
                    foreach (var dept in departmentDistribution)
                    {
                        viewModel.DepartmentDistribution.Add(new PieSeries
                        {
                            Title = dept.DepartmentName,
                            Values = new ChartValues<int> { dept.Count },
                            DataLabels = true
                        });
                    }

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