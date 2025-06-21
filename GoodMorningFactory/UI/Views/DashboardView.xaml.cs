using GoodMorningFactory.Data;
using GoodMorningFactory.UI.ViewModels;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using GoodMorningFactory.Core.Services; // *** بداية التعديل: إضافة using ***

namespace GoodMorningFactory.UI.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            // *** بداية التعديل: تعيين تنسيق العملة لمحور المبيعات بناءً على الإعدادات ***
            SalesAxisY.LabelFormatter = value => $"{value:N0} {AppSettings.DefaultCurrencySymbol}";
            // *** نهاية التعديل ***

            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var today = DateTime.Today;
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    var endOfToday = today.AddDays(1).AddTicks(-1);

                    var viewModel = new DashboardViewModel
                    {
                        TotalSalesToday = db.Sales.Where(s => s.SaleDate >= today && s.SaleDate <= endOfToday).Sum(s => (decimal?)s.TotalAmount) ?? 0,
                        TotalSalesThisMonth = db.Sales.Where(s => s.SaleDate >= startOfMonth && s.SaleDate <= endOfToday).Sum(s => (decimal?)s.TotalAmount) ?? 0,
                        TotalProducts = db.Products.Count(),
                        LowStockProducts = db.Inventories.Count(i => i.Quantity <= 5)
                    };

                    var topProducts = db.SaleItems.GroupBy(si => si.Product.Name)
                        .Select(g => new { ProductName = g.Key, TotalQuantity = g.Sum(si => si.Quantity) })
                        .OrderByDescending(x => x.TotalQuantity).Take(5).ToList();

                    viewModel.TopSellingProductsSeries = new SeriesCollection();
                    foreach (var product in topProducts)
                    {
                        viewModel.TopSellingProductsSeries.Add(new PieSeries
                        {
                            Title = product.ProductName,
                            Values = new ChartValues<int> { product.TotalQuantity },
                            DataLabels = true
                        });
                    }

                    var salesData = new ChartValues<decimal>();
                    var monthLabels = new List<string>();
                    for (int i = 5; i >= 0; i--)
                    {
                        var date = DateTime.Now.AddMonths(-i);
                        var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                        var monthlyTotal = db.Sales
                            .Where(s => s.SaleDate >= firstDayOfMonth && s.SaleDate <= lastDayOfMonth)
                            .Sum(s => (decimal?)s.TotalAmount) ?? 0;

                        salesData.Add(monthlyTotal);
                        monthLabels.Add(firstDayOfMonth.ToString("MMM yy", new CultureInfo("ar-EG")));
                    }

                    viewModel.MonthlySalesSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "إجمالي المبيعات",
                            Values = salesData
                        }
                    };
                    viewModel.MonthLabels = monthLabels.ToArray();

                    this.DataContext = viewModel;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل بيانات لوحة التحكم: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
