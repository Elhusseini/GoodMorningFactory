// UI/Views/SalesDashboardView.xaml.cs
// *** الكود الكامل للكود الخلفي للوحة معلومات المبيعات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class SalesDashboardView : UserControl
    {
        public SalesDashboardView()
        {
            InitializeComponent();
            SalesAxisY.LabelFormatter = value => value.ToString("C", new CultureInfo("ar-KW"));
            CategorySalesAxisY.LabelFormatter = value => value.ToString("C", new CultureInfo("ar-KW"));
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var viewModel = new SalesDashboardViewModel();
                    var today = DateTime.Today;
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                    // --- حساب مؤشرات الأداء الرئيسية ---
                    var salesThisMonth = db.Sales.Where(s => s.SaleDate >= startOfMonth && s.SaleDate <= endOfMonth).ToList();
                    viewModel.TotalSalesThisMonth = salesThisMonth.Sum(s => s.TotalAmount);
                    viewModel.NewOrdersThisMonth = db.SalesOrders.Count(o => o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth);
                    viewModel.FollowUpQuotationsCount = db.SalesQuotations.Count(q => q.Status == QuotationStatus.Sent && q.ValidUntilDate >= today);
                    viewModel.AverageOrderValue = salesThisMonth.Any() ? salesThisMonth.Average(s => s.TotalAmount) : 0;

                    // --- حساب أفضل العملاء والمنتجات ---
                    var customerSales = db.Sales.Include(s => s.SalesOrder.Customer).Where(s => s.SaleDate.Year == today.Year && s.SalesOrder != null).GroupBy(s => s.SalesOrder.Customer.CustomerName).Select(g => new { Name = g.Key, Total = g.Sum(s => s.TotalAmount) }).ToList();
                    viewModel.TopCustomers = customerSales.OrderByDescending(x => x.Total).Take(5).Select(x => x.Name).ToList();
                    var productSales = db.SaleItems.Include(si => si.Product).Where(si => si.Sale.SaleDate.Year == today.Year).GroupBy(si => si.Product.Name).Select(g => new { Name = g.Key, Quantity = g.Sum(si => si.Quantity) }).ToList();
                    viewModel.TopProducts = productSales.OrderByDescending(x => x.Quantity).Take(5).Select(x => $"{x.Name} ({x.Quantity} قطعة)").ToList();

                    // --- إعداد بيانات الرسم البياني للمبيعات الشهرية ---
                    var monthlySalesData = new ChartValues<decimal>();
                    var monthLabels = new List<string>();
                    for (int i = 5; i >= 0; i--)
                    {
                        var date = DateTime.Now.AddMonths(-i);
                        var firstDay = new DateTime(date.Year, date.Month, 1);
                        var lastDay = firstDay.AddMonths(1).AddDays(-1);
                        var monthlyTotal = db.Sales.Where(s => s.SaleDate >= firstDay && s.SaleDate <= lastDay).Sum(s => (decimal?)s.TotalAmount) ?? 0;
                        monthlySalesData.Add(monthlyTotal);
                        monthLabels.Add(firstDay.ToString("MMM yy", new CultureInfo("ar-KW")));
                    }
                    viewModel.MonthlySalesSeries = new SeriesCollection { new ColumnSeries { Title = "إجمالي المبيعات", Values = monthlySalesData } };
                    viewModel.MonthLabels = monthLabels.ToArray();

                    // --- إعداد بيانات الرسم البياني للمبيعات حسب الفئة ---
                    var categorySales = db.SaleItems.Include(si => si.Product.Category).Where(si => si.Sale.SaleDate.Year == today.Year).GroupBy(si => si.Product.Category.Name).Select(g => new { CategoryName = g.Key, Total = g.Sum(si => si.Quantity * si.UnitPrice) }).ToList();
                    viewModel.SalesByCategorySeries = new SeriesCollection { new RowSeries { Title = "المبيعات", Values = new ChartValues<decimal>(categorySales.Select(c => c.Total)) } };
                    viewModel.CategoryLabels = categorySales.Select(c => c.CategoryName).ToArray();

                    // --- حساب بيانات قمع المبيعات ---
                    viewModel.QuotationsCount = db.SalesQuotations.Count(q => q.QuotationDate >= startOfMonth && q.QuotationDate <= endOfMonth);
                    viewModel.OrdersCount = db.SalesOrders.Count(o => o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth);
                    viewModel.InvoicesCount = db.Sales.Count(i => i.SaleDate >= startOfMonth && i.SaleDate <= endOfMonth);

                    this.DataContext = viewModel;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل بيانات لوحة المعلومات: {ex.Message}\n\n{ex.InnerException?.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- دوال الاختصارات السريعة ---
        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateTo("Customers");
        }
        private void AddQuotation_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateTo("Quotations");
        }
        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateTo("Orders");
        }
    }
}