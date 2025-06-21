// UI/Views/AddSalesReturnWindow.xaml.cs
// *** تحديث: إضافة منطق تسجيل حركة "مرتجع مبيعات" في السجل المركزي ***
using GoodMorningFactory.Core.Services;
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
                    // نفترض وجود موقع افتراضي للمرتجعات
                    var defaultLocation = db.StorageLocations.FirstOrDefault();
                    if (defaultLocation == null)
                    {
                        throw new Exception("لا يوجد موقع تخزين افتراضي معرف في النظام لاستلام المرتجعات.");
                    }

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

                            var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId && i.StorageLocationId == defaultLocation.Id);
                            if (inventory != null) { inventory.Quantity += item.QuantityToReturn; }
                            else { db.Inventories.Add(new Inventory { ProductId = item.ProductId, StorageLocationId = defaultLocation.Id, Quantity = item.QuantityToReturn }); }

                            // --- بداية الإضافة: تسجيل حركة مرتجع المبيعات ---
                            var product = db.Products.Find(item.ProductId);
                            db.StockMovements.Add(new StockMovement
                            {
                                ProductId = item.ProductId,
                                StorageLocationId = defaultLocation.Id,
                                MovementDate = salesReturn.ReturnDate,
                                MovementType = StockMovementType.SalesReturn,
                                Quantity = item.QuantityToReturn,
                                UnitCost = product.AverageCost,
                                ReferenceDocument = salesReturn.ReturnNumber,
                                UserId = CurrentUserService.LoggedInUser?.Id
                            });
                            // --- نهاية الإضافة ---
                        }
                    }

                    if (!somethingWasReturned) { MessageBox.Show("لم يتم تحديد أي كميات للإرجاع."); return; }

                    salesReturn.TotalReturnValue = totalReturnValue;
                    db.SalesReturns.Add(salesReturn);

                    var originalSale = db.Sales.Include(s => s.Customer).FirstOrDefault(s => s.Id == _saleId);
                    if (originalSale != null)
                    {
                        originalSale.TotalAmount -= totalReturnValue;
                        if (originalSale.AmountPaid > originalSale.TotalAmount)
                        {
                            originalSale.AmountPaid = originalSale.TotalAmount;
                        }
                        UpdateInvoiceStatus(originalSale);
                    }

                    var companyInfo = db.CompanyInfos.FirstOrDefault();
                    var salesAccount = db.Accounts.Find(companyInfo?.DefaultSalesAccountId);
                    var arAccount = db.Accounts.Find(companyInfo?.DefaultAccountsReceivableAccountId);

                    if (salesAccount == null || arAccount == null)
                    {
                        throw new Exception("لا يمكن إنشاء القيد المحاسبي. يرجى التأكد من وجود حساب افتراضي للمبيعات وحساب للذمم المدينة في الإعدادات.");
                    }

                    var creditNoteVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"CN-{salesReturn.ReturnNumber}",
                        VoucherDate = DateTime.Today,
                        Description = $"إشعار دائن لمرتجع من الفاتورة رقم {originalSale.InvoiceNumber}",
                        TotalDebit = totalReturnValue,
                        TotalCredit = totalReturnValue,
                        Status = VoucherStatus.Posted
                    };

                    creditNoteVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = salesAccount.Id, Debit = totalReturnValue, Credit = 0 });
                    creditNoteVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = arAccount.Id, Debit = 0, Credit = totalReturnValue });

                    db.JournalVouchers.Add(creditNoteVoucher);

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم تسجيل المرتجع والإشعار الدائن بنجاح.", "نجاح");
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
                }
            }
        }

        private void UpdateInvoiceStatus(Sale sale)
        {
            if (sale.AmountPaid >= sale.TotalAmount)
            {
                sale.Status = InvoiceStatus.Paid;
            }
            else if (sale.AmountPaid > 0)
            {
                sale.Status = InvoiceStatus.PartiallyPaid;
            }
        }
    }
}
