// UI/Views/AddPurchaseReturnWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إنشاء مرتجع مشتريات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public class PurchaseReturnItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OriginalQuantity { get; set; }
        public int QuantityToReturn { get; set; }
        public decimal UnitPrice { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class AddPurchaseReturnWindow : Window
    {
        private readonly int _purchaseId;
        private List<PurchaseReturnItemViewModel> _itemsToReturn = new List<PurchaseReturnItemViewModel>();

        public AddPurchaseReturnWindow(int purchaseId)
        {
            InitializeComponent();
            _purchaseId = purchaseId;
            ReturnItemsDataGrid.ItemsSource = _itemsToReturn;
            LoadInvoiceData();
        }

        private void LoadInvoiceData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var purchase = db.Purchases.Find(_purchaseId);
                    if (purchase == null) { this.Close(); return; }

                    InvoiceNumberTextBlock.Text = purchase.InvoiceNumber;

                    var invoiceItems = db.PurchaseItems.Where(i => i.PurchaseId == _purchaseId).ToList();

                    foreach (var item in invoiceItems)
                    {
                        _itemsToReturn.Add(new PurchaseReturnItemViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = db.Products.Find(item.ProductId)?.Name,
                            OriginalQuantity = item.Quantity,
                            QuantityToReturn = 0,
                            UnitPrice = item.UnitPrice
                        });
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ: {ex.Message}"); }
        }

        private void ConfirmReturnButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var purchaseReturn = new PurchaseReturn
                    {
                        ReturnNumber = $"PRTN-{DateTime.Now:yyyyMMddHHmmss}",
                        ReturnDate = DateTime.Today,
                        PurchaseId = _purchaseId,
                    };

                    decimal totalReturnValue = 0;
                    foreach (var item in _itemsToReturn)
                    {
                        if (item.QuantityToReturn > 0)
                        {
                            if (item.QuantityToReturn > item.OriginalQuantity) { throw new Exception($"لا يمكن إرجاع كمية أكبر من المشتراة للمنتج: {item.ProductName}"); }

                            purchaseReturn.PurchaseReturnItems.Add(new PurchaseReturnItem { ProductId = item.ProductId, Quantity = item.QuantityToReturn });
                            totalReturnValue += item.QuantityToReturn * item.UnitPrice;

                            var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                            if (inventory != null) { inventory.Quantity -= item.QuantityToReturn; }
                        }
                    }

                    if (totalReturnValue == 0) { MessageBox.Show("لم يتم تحديد أي كميات للإرجاع."); return; }

                    purchaseReturn.TotalReturnValue = totalReturnValue;
                    db.PurchaseReturns.Add(purchaseReturn);

                    var originalPurchase = db.Purchases.Find(_purchaseId);
                    if (originalPurchase != null) { originalPurchase.TotalAmount -= totalReturnValue; }

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم تسجيل المرتجع بنجاح.", "نجاح");
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