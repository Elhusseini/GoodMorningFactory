// UI/Views/StockMovementsView.xaml.cs
// *** تحديث: تم تعديل الكود ليعمل مع الـ ViewModel المركزي ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels; // استخدام الـ ViewModel المركزي
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class StockMovementsView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 25;
        private int _totalItems = 0;

        public StockMovementsView()
        {
            InitializeComponent();
            LoadFilters();
            LoadMovements();
        }

        private void LoadFilters()
        {
            var types = new List<object> { "الكل" };
            types.AddRange(Enum.GetValues(typeof(StockMovementType)).Cast<object>());
            TypeFilterComboBox.ItemsSource = types;
            TypeFilterComboBox.SelectedIndex = 0;

            using (var db = new DatabaseContext())
            {
                var products = new List<object> { new { Name = "الكل", Id = 0 } };
                products.AddRange(db.Products.OrderBy(p => p.Name).ToList());
                ProductFilterComboBox.ItemsSource = products;
                ProductFilterComboBox.SelectedIndex = 0;

                // إضافة فلتر المخزن
                var warehouses = new List<object> { new { Name = "الكل", Id = 0 } };
                warehouses.AddRange(db.Warehouses.Where(w => w.IsActive).ToList());
                WarehouseFilterComboBox.ItemsSource = warehouses;
                WarehouseFilterComboBox.SelectedIndex = 0;
            }
        }

        private void LoadMovements()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.StockMovements
                                  .Include(m => m.Product)
                                  .Include(m => m.StorageLocation.Warehouse)
                                  .Include(m => m.User)
                                  .AsQueryable();

                    // تطبيق الفلاتر
                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(m => m.ReferenceDocument.ToLower().Contains(searchText) || m.Product.Name.ToLower().Contains(searchText));
                    }
                    if (TypeFilterComboBox.SelectedIndex > 0 && TypeFilterComboBox.SelectedItem is StockMovementType type)
                    {
                        query = query.Where(m => m.MovementType == type);
                    }
                    if (ProductFilterComboBox.SelectedIndex > 0)
                    {
                        int productId = (int)ProductFilterComboBox.SelectedValue;
                        query = query.Where(m => m.ProductId == productId);
                    }
                    if (WarehouseFilterComboBox.SelectedIndex > 0)
                    {
                        int warehouseId = (int)WarehouseFilterComboBox.SelectedValue;
                        query = query.Where(m => m.StorageLocation.WarehouseId == warehouseId);
                    }
                    if (FromDatePicker.SelectedDate.HasValue)
                    {
                        query = query.Where(m => m.MovementDate.Date >= FromDatePicker.SelectedDate.Value.Date);
                    }
                    if (ToDatePicker.SelectedDate.HasValue)
                    {
                        query = query.Where(m => m.MovementDate.Date <= ToDatePicker.SelectedDate.Value.Date);
                    }

                    _totalItems = query.Count();

                    var movements = query.OrderByDescending(m => m.MovementDate)
                                         .Skip((_currentPage - 1) * _pageSize)
                                         .Take(_pageSize)
                                         .ToList();

                    var viewModels = movements.Select(m => new StockMovementViewModel
                    {
                        Date = m.MovementDate,
                        MovementType = m.MovementType,
                        ReferenceNumber = m.ReferenceDocument,
                        ProductName = m.Product.Name,
                        WarehouseName = m.StorageLocation.Warehouse.Name,
                        StorageLocationName = m.StorageLocation.Name,
                        QuantityIn = (m.MovementType == StockMovementType.PurchaseReceipt || m.MovementType == StockMovementType.FinishedGoodsProduction || m.MovementType == StockMovementType.AdjustmentIncrease || m.MovementType == StockMovementType.TransferIn || m.MovementType == StockMovementType.SalesReturn) ? m.Quantity : 0,
                        QuantityOut = (m.MovementType == StockMovementType.SalesShipment || m.MovementType == StockMovementType.ProductionConsumption || m.MovementType == StockMovementType.AdjustmentDecrease || m.MovementType == StockMovementType.TransferOut || m.MovementType == StockMovementType.PurchaseReturn) ? m.Quantity : 0,
                        UserName = m.User?.Username ?? "System"
                    }).ToList();

                    MovementsDataGrid.ItemsSource = viewModels;
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل حركات المخزون: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadMovements(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage < (int)Math.Ceiling((double)_totalItems / _pageSize)) { _currentPage++; LoadMovements(); } }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadMovements(); } }
        private void Filter_Changed(object sender, RoutedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadMovements(); } }
        private void Filter_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { _currentPage = 1; LoadMovements(); } }
    }
}
