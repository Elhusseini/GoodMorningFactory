// UI/Views/StockMovementsView.xaml.cs
// *** الكود الكامل للكود الخلفي لواجهة حركات المخزون ***
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
            TypeFilterComboBox.ItemsSource = new List<string> { "الكل", "استلام بضاعة", "مرتجع مبيعات", "إنتاج مكتمل", "صرف إنتاج", "مبيعات" };
            TypeFilterComboBox.SelectedIndex = 0;
        }

        private void LoadMovements()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // جلب جميع أنواع الحركات
                    var goodsReceipts = db.GoodsReceiptNoteItems.Include(i => i.GoodsReceiptNote.PurchaseOrder).Include(i => i.Product)
                                        .Select(i => new { Date = i.GoodsReceiptNote.ReceiptDate, Type = "استلام بضاعة", Ref = i.GoodsReceiptNote.GRNNumber, ProdName = i.Product.Name, QtyIn = i.QuantityReceived, QtyOut = 0, User = "System" });

                    var salesShipments = db.ShipmentItems.Include(i => i.Shipment.SalesOrder).Include(i => i.Product)
                                        .Select(i => new { Date = i.Shipment.ShipmentDate, Type = "مبيعات", Ref = i.Shipment.ShipmentNumber, ProdName = i.Product.Name, QtyIn = 0, QtyOut = i.Quantity, User = "System" });

                    var productionConsumption = db.WorkOrderMaterials.Include(m => m.WorkOrder).Include(m => m.RawMaterial)
                                               .Select(m => new { Date = m.WorkOrder.ActualStartDate ?? m.WorkOrder.PlannedStartDate, Type = "صرف إنتاج", Ref = m.WorkOrder.WorkOrderNumber, ProdName = m.RawMaterial.Name, QtyIn = 0, QtyOut = (int)m.QuantityConsumed, User = "System" });

                    var finishedGoodsProduction = db.WorkOrders
                        .Where(wo => wo.QuantityProduced > 0 && wo.ActualEndDate.HasValue)
                        .Include(wo => wo.FinishedGood)
                        .Select(wo => new { Date = wo.ActualEndDate.Value, Type = "إنتاج مكتمل", Ref = wo.WorkOrderNumber, ProdName = wo.FinishedGood.Name, QtyIn = wo.QuantityProduced, QtyOut = 0, User = "System" });

                    var allMovements = goodsReceipts.Union(salesShipments).Union(productionConsumption).Union(finishedGoodsProduction);

                    // تطبيق الفلاتر
                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        allMovements = allMovements.Where(m => m.Ref.ToLower().Contains(searchText) || m.ProdName.ToLower().Contains(searchText));
                    }
                    if (TypeFilterComboBox.SelectedIndex > 0)
                    {
                        string type = TypeFilterComboBox.SelectedItem.ToString();
                        allMovements = allMovements.Where(m => m.Type == type);
                    }
                    // (يمكن إضافة فلتر التاريخ هنا بنفس الطريقة)

                    _totalItems = allMovements.Count();
                    MovementsDataGrid.ItemsSource = allMovements.OrderByDescending(m => m.Date).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
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