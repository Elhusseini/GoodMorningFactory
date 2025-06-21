// UI/Views/AddPurchaseReturnWindow.xaml.cs
// *** تحديث: إضافة منطق تسجيل حركة "مرتجع مشتريات" في السجل المركزي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;

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
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
                    var companyInfo = db.CompanyInfos.FirstOrDefault();
                    if (companyInfo?.DefaultPurchaseReturnsAccountId == null || companyInfo?.DefaultAccountsPayableAccountId == null)
                    {
                        throw new Exception("يرجى تحديد حساب 'مردودات المشتريات' و 'الذمم الدائنة' الافتراضي في شاشة الإعدادات.");
                    }

                    // نفترض وجود موقع افتراضي للمرتجعات
                    var defaultLocation = db.StorageLocations.FirstOrDefault();
                    if (defaultLocation == null)
                    {
                        throw new Exception("لا يوجد موقع تخزين افتراضي معرف في النظام لإرجاع البضاعة منه.");
                    }

                    var purchaseReturn = new PurchaseReturn
                    {
                        ReturnNumber = $"PRTN-{DateTime.Now:yyyyMMddHHmmss}",
                        ReturnDate = DateTime.Today,
                        PurchaseId = _purchaseId,
                    };

                    bool somethingWasReturned = false;
                    decimal totalReturnValue = 0;

                    foreach (var item in _itemsToReturn)
                    {
                        if (item.QuantityToReturn > 0)
                        {
                            if (item.QuantityToReturn > item.OriginalQuantity) { throw new Exception($"لا يمكن إرجاع كمية أكبر من المشتراة للمنتج: {item.ProductName}"); }

                            somethingWasReturned = true;
                            purchaseReturn.PurchaseReturnItems.Add(new PurchaseReturnItem { ProductId = item.ProductId, Quantity = item.QuantityToReturn });
                            totalReturnValue += item.QuantityToReturn * item.UnitPrice;

                            var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId && i.StorageLocationId == defaultLocation.Id);
                            if (inventory != null)
                            {
                                inventory.Quantity -= item.QuantityToReturn;
                            }

                            // --- بداية الإضافة: تسجيل حركة مرتجع المشتريات ---
                            var product = db.Products.Find(item.ProductId);
                            db.StockMovements.Add(new StockMovement
                            {
                                ProductId = item.ProductId,
                                StorageLocationId = defaultLocation.Id,
                                MovementDate = purchaseReturn.ReturnDate,
                                MovementType = StockMovementType.PurchaseReturn,
                                Quantity = item.QuantityToReturn,
                                UnitCost = product.AverageCost, // استخدام متوسط التكلفة لتسجيل قيمة الحركة
                                ReferenceDocument = purchaseReturn.ReturnNumber,
                                UserId = CurrentUserService.LoggedInUser?.Id
                            });
                            // --- نهاية الإضافة ---
                        }
                    }

                    if (!somethingWasReturned) { MessageBox.Show("لم يتم تحديد أي كميات للإرجاع."); return; }

                    purchaseReturn.TotalReturnValue = totalReturnValue;
                    db.PurchaseReturns.Add(purchaseReturn);

                    var originalPurchase = db.Purchases.Include(p => p.Supplier).FirstOrDefault(p => p.Id == _purchaseId);
                    if (originalPurchase != null)
                    {
                        originalPurchase.TotalAmount -= totalReturnValue;

                        if (originalPurchase.AmountPaid > originalPurchase.TotalAmount)
                        {
                            originalPurchase.AmountPaid = originalPurchase.TotalAmount;
                        }

                        if (originalPurchase.AmountPaid >= originalPurchase.TotalAmount)
                        {
                            originalPurchase.Status = PurchaseInvoiceStatus.FullyPaid;
                        }
                        else if (originalPurchase.AmountPaid > 0)
                        {
                            originalPurchase.Status = PurchaseInvoiceStatus.PartiallyPaid;
                        }
                        else
                        {
                            originalPurchase.Status = PurchaseInvoiceStatus.ApprovedForPayment;
                        }
                    }

                    var debitNoteVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"DN-{purchaseReturn.ReturnNumber}",
                        VoucherDate = DateTime.Today,
                        Description = $"إشعار مدين للمورد: {originalPurchase.Supplier.Name} لمرتجع من الفاتورة رقم: {originalPurchase.InvoiceNumber}",
                        TotalDebit = totalReturnValue,
                        TotalCredit = totalReturnValue,
                        Status = VoucherStatus.Posted
                    };

                    debitNoteVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsPayableAccountId.Value, Debit = totalReturnValue, Credit = 0 });
                    debitNoteVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultPurchaseReturnsAccountId.Value, Debit = 0, Credit = totalReturnValue });

                    db.JournalVouchers.Add(debitNoteVoucher);

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم تسجيل المرتجع والإشعار المدين بنجاح.", "نجاح");
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
