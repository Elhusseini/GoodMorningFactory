// UI/Views/AddGoodsReceiptWindow.xaml.cs
// *** تحديث: إضافة منطق لإنشاء سجل حركة مخزون مركزي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddGoodsReceiptWindow : Window
    {
        private readonly int _purchaseOrderId;
        private List<GoodsReceiptItemViewModel> _itemsToReceive = new List<GoodsReceiptItemViewModel>();

        public AddGoodsReceiptWindow(int purchaseOrderId)
        {
            InitializeComponent();
            _purchaseOrderId = purchaseOrderId;
            ReceiptItemsDataGrid.ItemsSource = _itemsToReceive;
            LoadPurchaseOrderData();
        }

        private void LoadPurchaseOrderData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var po = db.PurchaseOrders.Find(_purchaseOrderId);
                    if (po == null) { this.Close(); return; }
                    PurchaseOrderNumberTextBlock.Text = $"خاص بأمر الشراء رقم: {po.PurchaseOrderNumber}";

                    var allLocations = db.StorageLocations.Where(l => l.IsActive).ToList();
                    var orderItems = db.PurchaseOrderItems.Include(i => i.Product).Where(i => i.PurchaseOrderId == _purchaseOrderId).ToList();
                    var receivedItems = db.GoodsReceiptNotes
                                          .Where(grn => grn.PurchaseOrderId == _purchaseOrderId)
                                          .SelectMany(grn => grn.GoodsReceiptNoteItems)
                                          .GroupBy(gri => gri.ProductId)
                                          .ToDictionary(g => g.Key, g => g.Sum(i => i.QuantityReceived));

                    foreach (var item in orderItems)
                    {
                        int previouslyReceived = receivedItems.ContainsKey(item.ProductId) ? receivedItems[item.ProductId] : 0;
                        int remaining = item.Quantity - previouslyReceived;

                        _itemsToReceive.Add(new GoodsReceiptItemViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Product.Name,
                            OrderedQuantity = item.Quantity,
                            PreviouslyReceivedQuantity = previouslyReceived,
                            QuantityReceived = remaining > 0 ? remaining : 0,
                            UnitPrice = item.UnitPrice,
                            AvailableLocations = allLocations,
                            DestinationLocationId = allLocations.FirstOrDefault()?.Id,
                            IsTracked = item.Product.TrackingMethod != ProductTrackingMethod.None,
                            TrackingMethod = item.Product.TrackingMethod
                        });
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل بيانات الأمر: {ex.Message}", "خطأ"); }
        }

        private void EnterTrackingDataButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GoodsReceiptItemViewModel item)
            {
                if (item.QuantityReceived <= 0)
                {
                    MessageBox.Show("يرجى إدخال الكمية المستلمة أولاً.", "تنبيه");
                    return;
                }

                var trackingWindow = new EnterTrackingDataWindow(item.TrackingMethod, item.QuantityReceived);
                if (trackingWindow.ShowDialog() == true)
                {
                    if (item.TrackingMethod == ProductTrackingMethod.BySerialNumber)
                    {
                        item.SerialNumbers = trackingWindow.SerialNumbers.ToList();
                    }
                    else
                    {
                        item.LotInfo = new LotNumberInfo
                        {
                            Value = trackingWindow.LotNumber,
                            ExpiryDate = trackingWindow.ExpiryDate
                        };
                    }
                }
            }
        }

        private void ConfirmReceiptButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var grn = new GoodsReceiptNote
                    {
                        GRNNumber = $"GRN-{DateTime.Now:yyyyMMddHHmmss}",
                        ReceiptDate = DateTime.Today,
                        PurchaseOrderId = _purchaseOrderId
                    };
                    db.GoodsReceiptNotes.Add(grn);
                    db.SaveChanges(); // حفظ المذكرة أولاً للحصول على هوية

                    foreach (var itemVM in _itemsToReceive)
                    {
                        if (itemVM.QuantityReceived <= 0) continue;
                        if (itemVM.DestinationLocationId == null) throw new Exception($"يجب تحديد موقع التخزين للمنتج: {itemVM.ProductName}");

                        if (itemVM.IsTracked)
                        {
                            if (itemVM.TrackingMethod == ProductTrackingMethod.BySerialNumber && itemVM.SerialNumbers.Count != itemVM.QuantityReceived)
                                throw new Exception($"لم يتم إدخال الأرقام التسلسلية بشكل صحيح للمنتج: {itemVM.ProductName}");
                            if (itemVM.TrackingMethod == ProductTrackingMethod.ByLotNumber && itemVM.LotInfo == null)
                                throw new Exception($"لم يتم إدخال معلومات الدفعة للمنتج: {itemVM.ProductName}");
                        }

                        var grnItem = new GoodsReceiptNoteItem
                        {
                            GoodsReceiptNoteId = grn.Id,
                            ProductId = itemVM.ProductId,
                            QuantityReceived = itemVM.QuantityReceived
                        };
                        db.GoodsReceiptNoteItems.Add(grnItem);
                        db.SaveChanges();

                        if (itemVM.IsTracked)
                        {
                            if (itemVM.TrackingMethod == ProductTrackingMethod.BySerialNumber)
                            {
                                foreach (var serial in itemVM.SerialNumbers)
                                {
                                    db.SerialNumbers.Add(new SerialNumber
                                    {
                                        ProductId = itemVM.ProductId,
                                        Value = serial,
                                        StorageLocationId = itemVM.DestinationLocationId.Value,
                                        GoodsReceiptNoteItemId = grnItem.Id,
                                        Status = SerialNumberStatus.InStock
                                    });
                                }
                            }
                            else
                            {
                                db.LotNumbers.Add(new LotNumber
                                {
                                    ProductId = itemVM.ProductId,
                                    Value = itemVM.LotInfo.Value,
                                    CurrentQuantity = itemVM.QuantityReceived,
                                    ExpiryDate = itemVM.LotInfo.ExpiryDate,
                                    StorageLocationId = itemVM.DestinationLocationId.Value,
                                    GoodsReceiptNoteItemId = grnItem.Id
                                });
                            }
                        }

                        var product = db.Products.Find(itemVM.ProductId);
                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == itemVM.ProductId && i.StorageLocationId == itemVM.DestinationLocationId.Value);
                        if (inventory != null)
                        {
                            var oldTotalValue = inventory.Quantity * product.AverageCost;
                            var newTotalValue = itemVM.QuantityReceived * itemVM.UnitPrice;
                            var totalQuantity = inventory.Quantity + itemVM.QuantityReceived;
                            if (totalQuantity > 0) product.AverageCost = (oldTotalValue + newTotalValue) / totalQuantity;
                            inventory.Quantity += itemVM.QuantityReceived;
                        }
                        else
                        {
                            db.Inventories.Add(new Inventory { ProductId = itemVM.ProductId, StorageLocationId = itemVM.DestinationLocationId.Value, Quantity = itemVM.QuantityReceived });
                            product.AverageCost = itemVM.UnitPrice;
                        }

                        // --- بداية الإضافة: تسجيل الحركة في السجل المركزي ---
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = itemVM.ProductId,
                            StorageLocationId = itemVM.DestinationLocationId.Value,
                            MovementDate = grn.ReceiptDate,
                            MovementType = StockMovementType.PurchaseReceipt,
                            Quantity = itemVM.QuantityReceived,
                            UnitCost = itemVM.UnitPrice,
                            ReferenceDocument = grn.GRNNumber,
                            UserId = CurrentUserService.LoggedInUser?.Id
                        });
                        // --- نهاية الإضافة ---
                    }

                    var poToUpdate = db.PurchaseOrders.Include(p => p.PurchaseOrderItems).FirstOrDefault(p => p.Id == _purchaseOrderId);
                    var totalOrdered = poToUpdate.PurchaseOrderItems.Sum(i => i.Quantity);
                    var totalReceived = db.GoodsReceiptNoteItems.Where(gri => gri.GoodsReceiptNote.PurchaseOrderId == _purchaseOrderId).Sum(gri => gri.QuantityReceived);
                    if (totalReceived >= totalOrdered) poToUpdate.ReceiptStatus = ReceiptStatus.FullyReceived;
                    else poToUpdate.ReceiptStatus = ReceiptStatus.PartiallyReceived;

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم تسجيل استلام البضاعة وبيانات التتبع بنجاح.", "نجاح");
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
