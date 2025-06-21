// UI/Views/InventoryView.xaml.cs
// *** تحديث: تفعيل زر التعديل السريع وربطه بالنافذة الجديدة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
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

                var warehouses = new List<object> { new { Name = "الكل", Id = 0 } };
                warehouses.AddRange(db.Warehouses.ToList());
                WarehouseFilterComboBox.ItemsSource = warehouses;
                WarehouseFilterComboBox.SelectedIndex = 0;
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
                    var query = db.Inventories
                        .Include(i => i.Product).ThenInclude(p => p.Category)
                        .Include(i => i.StorageLocation).ThenInclude(sl => sl.Warehouse)
                        .AsQueryable();

                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(i => i.Product.Name.ToLower().Contains(searchText) || i.Product.ProductCode.ToLower().Contains(searchText));
                    }

                    if (WarehouseFilterComboBox.SelectedIndex > 0)
                    {
                        int warehouseId = (int)WarehouseFilterComboBox.SelectedValue;
                        query = query.Where(i => i.StorageLocation.WarehouseId == warehouseId);
                    }

                    if (CategoryFilterComboBox.SelectedIndex > 0)
                    {
                        int categoryId = (int)CategoryFilterComboBox.SelectedValue;
                        query = query.Where(i => i.Product.CategoryId == categoryId);
                    }

                    var allItems = query
                        .Select(i => new InventoryViewModel
                        {
                            ProductId = i.ProductId,
                            StorageLocationId = i.StorageLocationId,
                            ProductCode = i.Product.ProductCode,
                            ProductName = i.Product.Name,
                            CategoryName = i.Product.Category.Name ?? "غير مصنف",
                            WarehouseName = i.StorageLocation.Warehouse.Name,
                            StorageLocationName = i.StorageLocation.Name,
                            QuantityOnHand = i.Quantity,
                            QuantityReserved = i.QuantityReserved,
                            ReorderLevel = i.ReorderLevel,
                            MinStockLevel = i.MinStockLevel,
                            MaxStockLevel = i.MaxStockLevel,
                            AverageCost = i.Product.AverageCost
                        }).ToList();

                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is StockStatus status)
                    {
                        allItems = allItems.Where(i => i.Status == status).ToList();
                    }

                    _totalItems = allItems.Count();
                    InventoryDataGrid.ItemsSource = allItems.OrderBy(i => i.ProductName).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
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

        // --- بداية التحديث: تفعيل زر التعديل السريع ---
        private void QuickAdjustButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is InventoryViewModel item)
            {
                var adjustWindow = new QuickStockAdjustWindow(item.ProductId, item.StorageLocationId);
                if (adjustWindow.ShowDialog() == true)
                {
                    // تحديث القائمة بعد إغلاق نافذة التعديل بنجاح
                    LoadInventoryReport();
                }
            }
        }
        // --- نهاية التحديث ---

        private void ViewHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is InventoryViewModel item)
            {
                var historyWindow = new ProductStockHistoryWindow(item.ProductId);
                historyWindow.Show();
            }
        }
    }
}
