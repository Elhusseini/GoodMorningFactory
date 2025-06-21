// UI/Views/InventoryDashboardView.xaml.cs
// *** ملف جديد: الكود الخلفي للوحة معلومات المخزون ***
using GoodMorningFactory.Data;
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
    public partial class InventoryDashboardView : UserControl
    {
        public InventoryDashboardView()
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
                    var viewModel = new InventoryDashboardViewModel();
                    var inventoryItems = db.Inventories.Include(i => i.Product).ThenInclude(p => p.Category).ToList();

                    // --- حساب مؤشرات الأداء الرئيسية ---
                    viewModel.TotalInventoryValue = inventoryItems.Sum(i => i.Quantity * i.Product.PurchasePrice);
                    viewModel.LowStockItems = inventoryItems.Count(i => i.Quantity > 0 && i.Quantity <= i.ReorderLevel);
                    viewModel.OutOfStockItems = inventoryItems.Count(i => i.Quantity <= 0);

                    // --- إعداد بيانات الرسم البياني ---
                    var valueByCategory = inventoryItems
                        .GroupBy(i => i.Product.Category.Name ?? "غير مصنف")
                        .Select(g => new
                        {
                            CategoryName = g.Key,
                            TotalValue = g.Sum(i => i.Quantity * i.Product.PurchasePrice)
                        })
                        .ToList();

                    viewModel.ValueByCategorySeries = new SeriesCollection();
                    foreach (var category in valueByCategory)
                    {
                        viewModel.ValueByCategorySeries.Add(new PieSeries
                        {
                            Title = category.CategoryName,
                            Values = new ChartValues<decimal> { category.TotalValue },
                            DataLabels = true,
                            LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation)
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