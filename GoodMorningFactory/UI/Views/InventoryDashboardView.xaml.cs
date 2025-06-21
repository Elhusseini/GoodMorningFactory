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
                    var allProducts = db.Products.Include(p => p.Category).ToList();
                    viewModel.OutOfStockItems = allProducts.Count(p =>
                    {
                        var inv = inventoryItems.FirstOrDefault(i => i.ProductId == p.Id);
                        return inv == null || inv.Quantity <= 0;
                    });
                    viewModel.LowStockItems = allProducts.Count(p =>
                    {
                        var inv = inventoryItems.FirstOrDefault(i => i.ProductId == p.Id);
                        return inv != null && inv.Quantity > 0 && inv.Quantity <= inv.ReorderLevel;
                    });
                    viewModel.TotalInventoryValue = inventoryItems.Sum(i => i.Quantity * i.Product.PurchasePrice);

                    // --- إعداد بيانات الرسم البياني ---
                    var allCategories = db.Categories.ToList();
                    var valueByCategory = allCategories
                        .Select(cat => new {
                            CategoryName = cat.Name ?? "غير مصنف",
                            TotalValue = inventoryItems.Where(i => i.Product.CategoryId == cat.Id).Sum(i => i.Quantity * i.Product.PurchasePrice)
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