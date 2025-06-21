// UI/Views/AdjustStockWindow.xaml.cs
// *** تحديث شامل: تم تعديل المنطق ليعمل على مستوى الموقع الفرعي ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GoodMorningFactory.Core.Services;

namespace GoodMorningFactory.UI.Views
{
    public partial class AdjustStockWindow : Window
    {
        private ObservableCollection<StockAdjustmentItemViewModel> _itemsToAdjust = new ObservableCollection<StockAdjustmentItemViewModel>();

        public AdjustStockWindow(int? productId = null, int? warehouseId = null)
        {
            InitializeComponent();
            AdjustmentDatePicker.SelectedDate = DateTime.Today;
            ItemsDataGrid.ItemsSource = _itemsToAdjust;
            _itemsToAdjust.CollectionChanged += (s, e) => { if (e.NewItems != null) foreach (INotifyPropertyChanged item in e.NewItems) item.PropertyChanged += Item_PropertyChanged; };
            LoadWarehouses();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // لا حاجة لتحديث الإجماليات هنا، يمكن حسابها عند الحفظ
        }

        private void LoadWarehouses()
        {
            using (var db = new DatabaseContext())
            {
                WarehouseComboBox.ItemsSource = db.Warehouses.Where(w => w.IsActive).ToList();
            }
        }

        private void WarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _itemsToAdjust.Clear();
            if (WarehouseComboBox.SelectedItem is Warehouse selectedWarehouse)
            {
                using (var db = new DatabaseContext())
                {
                    LocationComboBox.ItemsSource = db.StorageLocations.Where(l => l.WarehouseId == selectedWarehouse.Id && l.IsActive).ToList();
                    LocationComboBox.IsEnabled = true;
                }
            }
            else
            {
                LocationComboBox.ItemsSource = null;
                LocationComboBox.IsEnabled = false;
            }
            SearchProductTextBox.IsEnabled = false;
        }

        private void LocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _itemsToAdjust.Clear();
            SearchProductTextBox.IsEnabled = LocationComboBox.SelectedItem != null;
        }

        private void SearchProductTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || LocationComboBox.SelectedItem == null) return;

            string searchText = SearchProductTextBox.Text;
            int locationId = (int)LocationComboBox.SelectedValue;

            using (var db = new DatabaseContext())
            {
                var product = db.Products.FirstOrDefault(p => p.ProductCode.ToLower() == searchText.ToLower() || p.Name.ToLower().Contains(searchText.ToLower()));
                if (product != null)
                {
                    if (_itemsToAdjust.Any(i => i.ProductId == product.Id)) return;

                    var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == product.Id && i.StorageLocationId == locationId);
                    int systemQty = inventory?.Quantity ?? 0;

                    _itemsToAdjust.Add(new StockAdjustmentItemViewModel
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        SystemQuantity = systemQty,
                        ActualQuantity = systemQty,
                        UnitCost = product.AverageCost
                    });
                    SearchProductTextBox.Clear();
                }
            }
        }

        private void PostAdjustmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (LocationComboBox.SelectedItem == null || !_itemsToAdjust.Any())
            {
                MessageBox.Show("يرجى اختيار مخزن وموقع فرعي وإضافة منتجات للجرد.", "بيانات ناقصة");
                return;
            }

            int locationId = (int)LocationComboBox.SelectedValue;
            int warehouseId = (int)WarehouseComboBox.SelectedValue;

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var adjustment = new StockAdjustment
                    {
                        ReferenceNumber = $"ADJ-{DateTime.Now:yyyyMMddHHmmss}",
                        AdjustmentDate = AdjustmentDatePicker.SelectedDate ?? DateTime.Today,
                        WarehouseId = warehouseId, // لا يزال مهما للتقرير
                        Reason = "جرد وتعديل مخزون"
                    };

                    foreach (var item in _itemsToAdjust)
                    {
                        if (item.Difference == 0) continue;

                        adjustment.StockAdjustmentItems.Add(new StockAdjustmentItem
                        {
                            ProductId = item.ProductId,
                            SystemQuantity = item.SystemQuantity,
                            CountedQuantity = item.ActualQuantity
                        });

                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId && i.StorageLocationId == locationId);
                        if (inventory != null)
                        {
                            inventory.Quantity = item.ActualQuantity;
                        }
                        else
                        {
                            db.Inventories.Add(new Inventory
                            {
                                ProductId = item.ProductId,
                                StorageLocationId = locationId,
                                Quantity = item.ActualQuantity,
                            });
                        }
                    }

                    if (!adjustment.StockAdjustmentItems.Any())
                    {
                        MessageBox.Show("لم يتم إجراء أي تغييرات على الكميات.", "معلومة");
                        return;
                    }

                    db.StockAdjustments.Add(adjustment);
                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم ترحيل تعديلات المخزون بنجاح.", "نجاح");
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
                }
            }
        }
    }
}
