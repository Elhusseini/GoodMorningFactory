// UI/Views/StockTransferWindow.xaml.cs
// *** تحديث: إضافة منطق تسجيل حركات التحويل في السجل المركزي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class StockTransferWindow : Window
    {
        private ObservableCollection<StockTransferItemViewModel> _itemsToTransfer = new ObservableCollection<StockTransferItemViewModel>();

        public StockTransferWindow()
        {
            InitializeComponent();
            TransferItemsDataGrid.ItemsSource = _itemsToTransfer;
            LoadWarehouses();
        }

        private void LoadWarehouses()
        {
            using (var db = new DatabaseContext())
            {
                var warehouses = db.Warehouses.Where(w => w.IsActive).ToList();
                SourceWarehouseComboBox.ItemsSource = warehouses;
                DestinationWarehouseComboBox.ItemsSource = new ObservableCollection<Warehouse>(warehouses);
            }
        }

        private void SourceWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _itemsToTransfer.Clear();
            SourceLocationComboBox.ItemsSource = null;
            if (SourceWarehouseComboBox.SelectedItem is Warehouse selectedWarehouse)
            {
                using (var db = new DatabaseContext())
                {
                    SourceLocationComboBox.ItemsSource = db.StorageLocations.Where(l => l.WarehouseId == selectedWarehouse.Id && l.IsActive).ToList();
                    SourceLocationComboBox.IsEnabled = true;
                }
            }
            else
            {
                SourceLocationComboBox.IsEnabled = false;
            }
            SearchProductTextBox.IsEnabled = false;
        }

        private void DestinationWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DestinationLocationComboBox.ItemsSource = null;
            if (DestinationWarehouseComboBox.SelectedItem is Warehouse selectedWarehouse)
            {
                using (var db = new DatabaseContext())
                {
                    DestinationLocationComboBox.ItemsSource = db.StorageLocations.Where(l => l.WarehouseId == selectedWarehouse.Id && l.IsActive).ToList();
                    DestinationLocationComboBox.IsEnabled = true;
                }
            }
            else
            {
                DestinationLocationComboBox.IsEnabled = false;
            }
        }

        private void SourceLocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _itemsToTransfer.Clear();
            SearchProductTextBox.IsEnabled = SourceLocationComboBox.SelectedItem != null;
        }

        private void SearchProductTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || SourceLocationComboBox.SelectedItem == null) return;

            string searchText = SearchProductTextBox.Text;
            int sourceLocationId = (int)SourceLocationComboBox.SelectedValue;

            using (var db = new DatabaseContext())
            {
                var product = db.Products.FirstOrDefault(p => p.ProductCode.ToLower() == searchText.ToLower() || p.Name.ToLower().Contains(searchText.ToLower()));
                if (product != null)
                {
                    if (_itemsToTransfer.Any(i => i.ProductId == product.Id))
                    {
                        MessageBox.Show("المنتج موجود بالفعل في القائمة.", "معلومة");
                        return;
                    }

                    var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == product.Id && i.StorageLocationId == sourceLocationId);
                    int availableQty = inventory?.Quantity ?? 0;

                    _itemsToTransfer.Add(new StockTransferItemViewModel
                    {
                        ProductId = product.Id,
                        ProductCode = product.ProductCode,
                        ProductName = product.Name,
                        AvailableQuantity = availableQty,
                        QuantityToTransfer = 1
                    });
                    SearchProductTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("لم يتم العثور على المنتج.", "بحث");
                }
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is StockTransferItemViewModel item)
            {
                _itemsToTransfer.Remove(item);
            }
        }

        private void ExecuteTransferButton_Click(object sender, RoutedEventArgs e)
        {
            if (SourceLocationComboBox.SelectedValue == null || DestinationLocationComboBox.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار الموقع المصدر والموقع الهدف.", "بيانات ناقصة");
                return;
            }

            if ((int)SourceLocationComboBox.SelectedValue == (int)DestinationLocationComboBox.SelectedValue)
            {
                MessageBox.Show("لا يمكن التحويل إلى نفس الموقع.", "خطأ منطقي");
                return;
            }

            if (!_itemsToTransfer.Any(i => i.QuantityToTransfer > 0))
            {
                MessageBox.Show("يرجى إضافة منتجات وتحديد كميات للتحويل.", "بيانات ناقصة");
                return;
            }

            foreach (var item in _itemsToTransfer)
            {
                if (item.QuantityToTransfer > item.AvailableQuantity)
                {
                    MessageBox.Show($"الكمية المطلوبة للمنتج '{item.ProductName}' أكبر من الكمية المتاحة في الموقع المصدر.", "كمية غير كافية");
                    return;
                }
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var transfer = new StockTransfer
                    {
                        TransferNumber = $"TRN-{DateTime.Now:yyyyMMddHHmmss}",
                        TransferDate = DateTime.Today,
                        SourceStorageLocationId = (int)SourceLocationComboBox.SelectedValue,
                        DestinationStorageLocationId = (int)DestinationLocationComboBox.SelectedValue,
                        Status = StockTransferStatus.Completed,
                        UserId = CurrentUserService.LoggedInUser.Id
                    };

                    foreach (var itemVM in _itemsToTransfer)
                    {
                        if (itemVM.QuantityToTransfer <= 0) continue;

                        transfer.StockTransferItems.Add(new StockTransferItem
                        {
                            ProductId = itemVM.ProductId,
                            Quantity = itemVM.QuantityToTransfer
                        });

                        var sourceInventory = db.Inventories.First(i => i.ProductId == itemVM.ProductId && i.StorageLocationId == transfer.SourceStorageLocationId);
                        sourceInventory.Quantity -= itemVM.QuantityToTransfer;

                        var destInventory = db.Inventories.FirstOrDefault(i => i.ProductId == itemVM.ProductId && i.StorageLocationId == transfer.DestinationStorageLocationId);
                        if (destInventory != null)
                        {
                            destInventory.Quantity += itemVM.QuantityToTransfer;
                        }
                        else
                        {
                            db.Inventories.Add(new Inventory
                            {
                                ProductId = itemVM.ProductId,
                                StorageLocationId = transfer.DestinationStorageLocationId,
                                Quantity = itemVM.QuantityToTransfer
                            });
                        }

                        // --- بداية الإضافة: تسجيل حركات التحويل ---
                        var product = db.Products.Find(itemVM.ProductId);

                        // حركة الخروج
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = itemVM.ProductId,
                            StorageLocationId = transfer.SourceStorageLocationId,
                            MovementDate = transfer.TransferDate,
                            MovementType = StockMovementType.TransferOut,
                            Quantity = itemVM.QuantityToTransfer,
                            UnitCost = product.AverageCost,
                            ReferenceDocument = transfer.TransferNumber,
                            UserId = CurrentUserService.LoggedInUser?.Id
                        });

                        // حركة الدخول
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = itemVM.ProductId,
                            StorageLocationId = transfer.DestinationStorageLocationId,
                            MovementDate = transfer.TransferDate,
                            MovementType = StockMovementType.TransferIn,
                            Quantity = itemVM.QuantityToTransfer,
                            UnitCost = product.AverageCost,
                            ReferenceDocument = transfer.TransferNumber,
                            UserId = CurrentUserService.LoggedInUser?.Id
                        });
                        // --- نهاية الإضافة ---
                    }

                    db.StockTransfers.Add(transfer);
                    db.SaveChanges();
                    transaction.Commit();

                    MessageBox.Show("تم تنفيذ عملية التحويل بنجاح.", "نجاح");
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت عملية التحويل: {ex.Message}", "خطأ فادح");
                }
            }
        }
    }
}
