// UI/Views/AddGoodsReceiptWindow.xaml.cs
// *** الكود الكامل لنافذة تسجيل استلام البضائع ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public class GoodsReceiptItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public int PreviouslyReceivedQuantity { get; set; }
        public int QuantityReceived { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }

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
                    PurchaseOrderNumberTextBlock.Text = po.PurchaseOrderNumber;

                    var orderItems = db.PurchaseOrderItems.Where(i => i.PurchaseOrderId == _purchaseOrderId).ToList();
                    var receivedItems = db.GoodsReceiptNoteItems.Include(gri => gri.GoodsReceiptNote)
                                          .Where(gri => gri.GoodsReceiptNote.PurchaseOrderId == _purchaseOrderId)
                                          .GroupBy(gri => gri.ProductId)
                                          .ToDictionary(g => g.Key, g => g.Sum(i => i.QuantityReceived));

                    foreach (var item in orderItems)
                    {
                        int previouslyReceived = receivedItems.ContainsKey(item.ProductId) ? receivedItems[item.ProductId] : 0;
                        int remaining = item.Quantity - previouslyReceived;

                        _itemsToReceive.Add(new GoodsReceiptItemViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = db.Products.Find(item.ProductId)?.Name,
                            OrderedQuantity = item.Quantity,
                            PreviouslyReceivedQuantity = previouslyReceived,
                            QuantityReceived = remaining > 0 ? remaining : 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}");
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

                    bool somethingReceived = false;
                    foreach (var item in _itemsToReceive)
                    {
                        if (item.QuantityReceived > 0)
                        {
                            somethingReceived = true;
                            grn.GoodsReceiptNoteItems.Add(new GoodsReceiptNoteItem
                            {
                                ProductId = item.ProductId,
                                QuantityReceived = item.QuantityReceived
                            });

                            var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                            if (inventory != null) { inventory.Quantity += item.QuantityReceived; }
                            else { db.Inventories.Add(new Inventory { ProductId = item.ProductId, Quantity = item.QuantityReceived }); }
                        }
                    }

                    if (!somethingReceived) { MessageBox.Show("لم يتم إدخال أي كميات للاستلام."); return; }

                    db.GoodsReceiptNotes.Add(grn);

                    var poToUpdate = db.PurchaseOrders.Find(_purchaseOrderId);
                    var totalOrdered = db.PurchaseOrderItems.Where(i => i.PurchaseOrderId == _purchaseOrderId).Sum(i => i.Quantity);
                    var totalReceived = db.GoodsReceiptNoteItems.Include(gri => gri.GoodsReceiptNote).Where(gri => gri.GoodsReceiptNote.PurchaseOrderId == _purchaseOrderId).Sum(i => i.QuantityReceived) + _itemsToReceive.Sum(i => i.QuantityReceived);

                    if (totalReceived >= totalOrdered) { poToUpdate.Status = PurchaseOrderStatus.FullyReceived; }
                    else { poToUpdate.Status = PurchaseOrderStatus.PartiallyReceived; }

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم تسجيل استلام البضاعة وتحديث المخزون بنجاح.", "نجاح");
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