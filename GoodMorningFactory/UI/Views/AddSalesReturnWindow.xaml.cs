// UI/Views/AddSalesReturnWindow.xaml.cs
// *** الكود الكامل لملف الكود الخلفي لنافذة إنشاء مرتجع مع الإصلاح ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public class SalesReturnItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OriginalQuantity { get; set; }
        public int PreviouslyReturnedQuantity { get; set; }
        public int QuantityToReturn { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public partial class AddSalesReturnWindow : Window
    {
        private readonly int _saleId;
        private List<SalesReturnItemViewModel> _itemsToReturn = new List<SalesReturnItemViewModel>();

        public AddSalesReturnWindow(int saleId)
        {
            InitializeComponent();
            _saleId = saleId;
            LoadInvoiceData();
        }

        private void LoadInvoiceData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var sale = db.Sales.Find(_saleId);
                    if (sale == null) { this.Close(); return; }

                    InvoiceNumberTextBlock.Text = sale.InvoiceNumber;

                    var invoiceItems = db.SaleItems.Where(i => i.SaleId == _saleId).ToList();
                    var returnedItems = db.SalesReturnItems.Include(sri => sri.SalesReturn)
                                          .Where(sri => sri.SalesReturn.SaleId == _saleId)
                                          .GroupBy(sri => sri.ProductId)
                                          .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

                    foreach (var item in invoiceItems)
                    {
                        int previouslyReturned = returnedItems.ContainsKey(item.ProductId) ? returnedItems[item.ProductId] : 0;
                        _itemsToReturn.Add(new SalesReturnItemViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = db.Products.Find(item.ProductId)?.Name,
                            OriginalQuantity = item.Quantity,
                            PreviouslyReturnedQuantity = previouslyReturned,
                            QuantityToReturn = 0,
                            UnitPrice = item.UnitPrice
                        });
                    }
                    ReturnItemsDataGrid.ItemsSource = _itemsToReturn;
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
                    var salesReturn = new SalesReturn
                    {
                        ReturnNumber = $"RTN-{DateTime.Now:yyyyMMddHHmmss}",
                        ReturnDate = DateTime.Today,
                        SaleId = _saleId,
                    };

                    bool somethingWasReturned = false;
                    decimal totalReturnValue = 0;

                    foreach (var item in _itemsToReturn)
                    {
                        if (item.QuantityToReturn > 0)
                        {
                            if (item.QuantityToReturn > (item.OriginalQuantity - item.PreviouslyReturnedQuantity))
                            {
                                throw new Exception($"لا يمكن إرجاع كمية أكبر من المسموح بها للمنتج: {item.ProductName}");
                            }

                            somethingWasReturned = true;
                            salesReturn.SalesReturnItems.Add(new SalesReturnItem { ProductId = item.ProductId, Quantity = item.QuantityToReturn });
                            totalReturnValue += item.QuantityToReturn * item.UnitPrice;

                            var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                            if (inventory != null) { inventory.Quantity += item.QuantityToReturn; }
                            else { db.Inventories.Add(new Inventory { ProductId = item.ProductId, Quantity = item.QuantityToReturn }); }
                        }
                    }

                    if (!somethingWasReturned) { MessageBox.Show("لم يتم تحديد أي كميات للإرجاع."); return; }

                    salesReturn.TotalReturnValue = totalReturnValue;
                    db.SalesReturns.Add(salesReturn);

                    // *** بداية الإصلاح: تعديل رصيد الفاتورة الأصلية ***
                    var originalSale = db.Sales.Find(_saleId);
                    if (originalSale != null)
                    {
                        // خصم قيمة المرتجع من إجمالي الفاتورة
                        originalSale.TotalAmount -= totalReturnValue;
                        // إذا كان المبلغ المدفوع أكبر من الإجمالي الجديد، يتم تعديله ليكون مساوياً للإجمالي
                        if (originalSale.AmountPaid > originalSale.TotalAmount)
                        {
                            originalSale.AmountPaid = originalSale.TotalAmount;
                        }
                    }
                    // *** نهاية الإصلاح ***

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