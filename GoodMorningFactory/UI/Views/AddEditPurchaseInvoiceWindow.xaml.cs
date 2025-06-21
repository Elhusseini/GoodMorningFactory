// UI/Views/AddEditPurchaseInvoiceWindow.xaml.cs
// *** تحديث: تم إزالة منطق التحقق والإغلاق التلقائي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public class PurchaseInvoiceItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public partial class AddEditPurchaseInvoiceWindow : Window
    {
        private int? _purchaseOrderId;
        private readonly ObservableCollection<PurchaseInvoiceItemViewModel> _items = new ObservableCollection<PurchaseInvoiceItemViewModel>();
        private List<int> _grnIdsToInvoice = new List<int>();

        public AddEditPurchaseInvoiceWindow(int? purchaseOrderId = null, int? grnId = null)
        {
            InitializeComponent();
            _purchaseOrderId = purchaseOrderId;

            if (grnId.HasValue)
            {
                _grnIdsToInvoice.Add(grnId.Value);
            }

            InvoiceDatePicker.SelectedDate = DateTime.Today;
            InvoiceItemsDataGrid.ItemsSource = _items;
            _items.CollectionChanged += (s, e) => UpdateTotal();

            LoadInitialData();
            InvoiceNumberTextBox.Text = $"PINV-{DateTime.Now:yyyyMMddHHmmss}";
        }

        private void UpdateTotal()
        {
            decimal totalAmount = _items.Sum(i => i.Subtotal);
            if (TotalAmountTextBlock != null)
            {
                TotalAmountTextBlock.Text = $"{totalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
            }
        }

        private void LoadInitialData()
        {
            using (var db = new DatabaseContext())
            {
                SupplierComboBox.ItemsSource = db.Suppliers.ToList();

                int? effectivePoId = _purchaseOrderId;

                if (_grnIdsToInvoice.Any())
                {
                    var grn = db.GoodsReceiptNotes.Find(_grnIdsToInvoice.First());
                    if (grn != null) effectivePoId = grn.PurchaseOrderId;
                }

                if (!effectivePoId.HasValue) return;

                var po = db.PurchaseOrders
                           .Include(p => p.Supplier)
                           .Include(p => p.PurchaseOrderItems)
                           .FirstOrDefault(p => p.Id == effectivePoId.Value);

                if (po == null) { this.Close(); return; }

                Title = $"إنشاء فاتورة لأمر الشراء: {po.PurchaseOrderNumber}";
                SupplierComboBox.SelectedValue = po.SupplierId;
                SupplierComboBox.IsEnabled = false;

                var uninvoicedGrnItemsQuery = db.GoodsReceiptNoteItems
                    .Include(gri => gri.GoodsReceiptNote)
                    .Include(gri => gri.Product)
                    .Where(gri => gri.GoodsReceiptNote.PurchaseOrderId == effectivePoId.Value && !gri.GoodsReceiptNote.IsInvoiced);

                if (_grnIdsToInvoice.Any())
                {
                    uninvoicedGrnItemsQuery = uninvoicedGrnItemsQuery.Where(gri => _grnIdsToInvoice.Contains(gri.GoodsReceiptNoteId));
                }

                var uninvoicedGrnItems = uninvoicedGrnItemsQuery.ToList();

                // --- بداية الإصلاح: إزالة الإغلاق التلقائي من هنا ---
                // if (!uninvoicedGrnItems.Any())
                // {
                //     // MessageBox.Show("لا توجد بضاعة مستلمة وغير مفوترة لإنشاء فاتورة لها.", "تنبيه");
                //     // this.Close(); // هذا السطر هو سبب المشكلة
                //     return;
                // }
                // --- نهاية الإصلاح ---

                _grnIdsToInvoice = uninvoicedGrnItems.Select(i => i.GoodsReceiptNoteId).Distinct().ToList();

                var itemsToInvoice = uninvoicedGrnItems
                    .GroupBy(gri => new { gri.ProductId, gri.Product.Name })
                    .Select(g => new PurchaseInvoiceItemViewModel
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        Quantity = g.Sum(i => i.QuantityReceived),
                        UnitPrice = po.PurchaseOrderItems.FirstOrDefault(poi => poi.ProductId == g.Key.ProductId)?.UnitPrice ?? 0
                    })
                    .ToList();

                foreach (var item in itemsToInvoice)
                {
                    _items.Add(item);
                }

                InvoiceItemsDataGrid.IsReadOnly = true;
            }

            UpdateTotal();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SupplierComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(InvoiceNumberTextBox.Text) || !_items.Any(i => i.ProductId > 0))
            {
                MessageBox.Show("يرجى اختيار مورد، إدخال رقم الفاتورة، وإضافة أصناف.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var companyInfo = db.CompanyInfos.FirstOrDefault();
                    if (companyInfo?.DefaultInventoryAccountId == null || companyInfo?.DefaultAccountsPayableAccountId == null)
                    {
                        throw new Exception("يرجى تحديد حساب 'المخزون' و 'الذمم الدائنة' الافتراضي في شاشة الإعدادات أولاً.");
                    }

                    decimal totalAmount = _items.Sum(i => i.Subtotal);

                    var purchase = new Purchase
                    {
                        InvoiceNumber = InvoiceNumberTextBox.Text,
                        PurchaseDate = InvoiceDatePicker.SelectedDate ?? DateTime.Today,
                        DueDate = DueDatePicker.SelectedDate,
                        SupplierId = (int)SupplierComboBox.SelectedValue,
                        PurchaseOrderId = _purchaseOrderId,
                        Status = PurchaseInvoiceStatus.ApprovedForPayment,
                        TotalAmount = totalAmount,
                        AmountPaid = 0
                    };

                    foreach (var item in _items.Where(i => i.ProductId > 0 && i.Quantity > 0))
                    {
                        purchase.PurchaseItems.Add(new PurchaseItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
                    }
                    db.Purchases.Add(purchase);

                    if (_grnIdsToInvoice.Any())
                    {
                        var grnsToUpdate = db.GoodsReceiptNotes.Where(g => _grnIdsToInvoice.Contains(g.Id)).ToList();
                        foreach (var grn in grnsToUpdate)
                        {
                            grn.IsInvoiced = true;
                        }
                    }

                    if (_purchaseOrderId.HasValue)
                    {
                        var poToUpdate = db.PurchaseOrders.Find(_purchaseOrderId.Value);
                        if (poToUpdate != null)
                        {
                            var totalInvoicedAmount = db.Purchases
                                .Where(p => p.PurchaseOrderId == _purchaseOrderId)
                                .Sum(p => (decimal?)p.TotalAmount) ?? 0;
                            totalInvoicedAmount += totalAmount;

                            if (totalInvoicedAmount >= poToUpdate.TotalAmount)
                            {
                                poToUpdate.InvoicingStatus = POInvoicingStatus.FullyInvoiced;
                                poToUpdate.Status = PurchaseOrderStatus.Invoiced;
                            }
                            else
                            {
                                poToUpdate.InvoicingStatus = POInvoicingStatus.PartiallyInvoiced;
                            }
                        }
                    }

                    var journalVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"PI-{purchase.InvoiceNumber}",
                        VoucherDate = purchase.PurchaseDate,
                        Description = $"إثبات فاتورة شراء من المورد: {((Supplier)SupplierComboBox.SelectedItem).Name} - فاتورة رقم: {purchase.InvoiceNumber}",
                        TotalDebit = totalAmount,
                        TotalCredit = totalAmount,
                        Status = VoucherStatus.Posted
                    };

                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = totalAmount, Credit = 0 });
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsPayableAccountId.Value, Debit = 0, Credit = totalAmount });

                    db.JournalVouchers.Add(journalVoucher);

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم تسجيل فاتورة المورد والقيد المحاسبي بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشل تسجيل الفاتورة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
