// UI/Views/InventoryView.xaml.cs
// *** الكود الكامل للكود الخلفي لواجهة المخزون مع جميع التحسينات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class InventoryView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalItems = 0;

        public InventoryView()
        {
            InitializeComponent();
            LoadFilters();
            LoadInventoryReport();
        }

        private void LoadFilters()
        {
            using (var db = new DatabaseContext())
            {
                var categories = new List<object> { new { Name = "الكل", Id = 0 } };
                categories.AddRange(db.Categories.ToList());
                CategoryFilterComboBox.ItemsSource = categories;
                CategoryFilterComboBox.SelectedIndex = 0;
            }

            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(StockStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void LoadInventoryReport()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = from p in db.Products.Include(p => p.Category)
                                join i in db.Inventories on p.Id equals i.ProductId into gj
                                from subInv in gj.DefaultIfEmpty()
                                select new InventoryViewModel
                                {
                                    ProductId = p.Id,
                                    ProductCode = p.ProductCode,
                                    ProductName = p.Name,
                                    CategoryName = p.Category.Name ?? "غير مصنف",
                                    QuantityOnHand = subInv == null ? 0 : subInv.Quantity,
                                    ReorderLevel = subInv == null ? 0 : subInv.ReorderLevel
                                };

                    // تطبيق الفلاتر
                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(i => i.ProductName.ToLower().Contains(searchText) || i.ProductCode.ToLower().Contains(searchText));
                    }
                    if (CategoryFilterComboBox.SelectedIndex > 0)
                    {
                        int categoryId = (int)CategoryFilterComboBox.SelectedValue;
                        query = query.Where(i => db.Products.First(p => p.Id == i.ProductId).CategoryId == categoryId);
                    }
                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is StockStatus status)
                    {
                        query = query.Where(i => i.Status == status);
                    }

                    _totalItems = query.Count();
                    InventoryDataGrid.ItemsSource = query.OrderBy(i => i.ProductName).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المخزون: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages == 0) totalPages = 1;
            PageInfoTextBlock.Text = $"الصفحة {_currentPage} من {totalPages} (إجمالي السجلات: {_totalItems})";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < totalPages;
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadInventoryReport(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage < (int)Math.Ceiling((double)_totalItems / _pageSize)) { _currentPage++; LoadInventoryReport(); } }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadInventoryReport(); } }
        private void Filter_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { _currentPage = 1; LoadInventoryReport(); } }
        // --- بداية التحديث ---
        private void AdjustStockButton_Click(object sender, RoutedEventArgs e)
        {
            var adjustWindow = new AdjustStockWindow();
            if (adjustWindow.ShowDialog() == true)
            {
                LoadInventoryReport(); // تحديث القائمة بعد إجراء التعديل
            }
        }
        // --- نهاية التحديث ---
        // --- بداية التحديث ---
        private void ViewHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is InventoryViewModel item)
            {
                var historyWindow = new ProductStockHistoryWindow(item.ProductId);
                historyWindow.Show();
            }
        }
        // --- نهاية التحديث ---
    }
}