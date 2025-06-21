// UI/Views/AddShipmentWindow.xaml.cs
// *** تحديث: تمت إضافة منطق لإنشاء قيد محاسبي تلقائي بعد الشحن ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    // ViewModel لعرض بيانات بنود الشحنة في الواجهة
    public class ShipmentItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public int PreviouslyShippedQuantity { get; set; }
        public int QuantityToShip { get; set; }
        public decimal UnitPrice { get; set; } // لحساب قيمة الفاتورة
    }

    public partial class AddShipmentWindow : Window
    {
        private readonly int _salesOrderId;
        private List<ShipmentItemViewModel> _itemsToShip = new List<ShipmentItemViewModel>();

        public AddShipmentWindow(int salesOrderId)
        {
            InitializeComponent();
            _salesOrderId = salesOrderId;
            LoadOrderData();
        }

        private void LoadOrderData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var order = db.SalesOrders.Find(_salesOrderId);
                    if (order == null) { this.Close(); return; }

                    SalesOrderNumberTextBox.Text = order.SalesOrderNumber;
                    ShipmentDatePicker.SelectedDate = DateTime.Today;

                    var orderItems = db.SalesOrderItems.Where(i => i.SalesOrderId == _salesOrderId).ToList();
                    var shippedItems = db.ShipmentItems.Include(si => si.Shipment)
                                         .Where(si => si.Shipment.SalesOrderId == _salesOrderId)
                                         .GroupBy(si => si.ProductId)
                                         .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

                    foreach (var item in orderItems)
                    {
                        int previouslyShipped = shippedItems.ContainsKey(item.ProductId) ? shippedItems[item.ProductId] : 0;
                        int remaining = item.Quantity - previouslyShipped;

                        _itemsToShip.Add(new ShipmentItemViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = db.Products.Find(item.ProductId)?.Name,
                            OrderedQuantity = item.Quantity,
                            PreviouslyShippedQuantity = previouslyShipped,
                            QuantityToShip = remaining > 0 ? remaining : 0,
                            UnitPrice = item.UnitPrice // جلب السعر من أمر البيع
                        });
                    }
                    ShipmentItemsDataGrid.ItemsSource = _itemsToShip;
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل بيانات الأمر: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void ConfirmShipmentButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // --- جلب إعدادات الحسابات الافتراضية ---
                    var companyInfo = db.CompanyInfos.FirstOrDefault();
                    if (companyInfo?.DefaultSalesAccountId == null || companyInfo?.DefaultAccountsReceivableAccountId == null)
                    {
                        MessageBox.Show("يرجى تحديد حساب المبيعات وحساب العملاء الافتراضي في شاشة الإعدادات أولاً.", "إعدادات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var shipment = new Shipment
                    {
                        ShipmentNumber = $"SHP-{DateTime.Now:yyyyMMddHHmmss}",
                        ShipmentDate = ShipmentDatePicker.SelectedDate ?? DateTime.Today,
                        SalesOrderId = _salesOrderId,
                        Carrier = CarrierTextBox.Text,
                        TrackingNumber = TrackingNumberTextBox.Text,
                        Status = ShipmentStatus.Shipped
                    };

                    bool somethingWasShipped = false;
                    decimal invoiceTotal = 0;
                    var itemsToInvoice = new List<SaleItem>();

                    foreach (var item in _itemsToShip)
                    {
                        if (item.QuantityToShip > 0)
                        {
                            somethingWasShipped = true;
                            shipment.ShipmentItems.Add(new ShipmentItem { ProductId = item.ProductId, Quantity = item.QuantityToShip });

                            // إضافة البند إلى قائمة الفوترة
                            itemsToInvoice.Add(new SaleItem { ProductId = item.ProductId, Quantity = item.QuantityToShip, UnitPrice = item.UnitPrice });
                            invoiceTotal += item.QuantityToShip * item.UnitPrice;

                            var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                            if (inventory != null && inventory.Quantity >= item.QuantityToShip)
                            {
                                inventory.Quantity -= item.QuantityToShip;
                            }
                            else { throw new Exception($"الكمية غير كافية في المخزون للمنتج: {item.ProductName}"); }
                        }
                    }

                    if (!somethingWasShipped) { MessageBox.Show("لم يتم تحديد أي كميات للشحن.", "تنبيه"); return; }

                    db.Shipments.Add(shipment);

                    // --- إنشاء الفاتورة ---
                    var invoice = new Sale
                    {
                        InvoiceNumber = $"INV-{shipment.ShipmentNumber}",
                        SaleDate = shipment.ShipmentDate,
                        SalesOrderId = _salesOrderId,
                        TotalAmount = invoiceTotal,
                        AmountPaid = 0, // افتراضياً، الدفع يتم لاحقاً
                        SaleItems = itemsToInvoice
                    };
                    db.Sales.Add(invoice);

                    // --- بداية التحديث: إنشاء القيد المحاسبي التلقائي ---
                    var journalVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"JV-{invoice.InvoiceNumber}",
                        VoucherDate = invoice.SaleDate,
                        Description = $"قيد إثبات مبيعات الفاتورة رقم {invoice.InvoiceNumber}",
                        TotalDebit = invoiceTotal,
                        TotalCredit = invoiceTotal
                    };
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsReceivableAccountId.Value, Debit = invoiceTotal, Credit = 0 });
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultSalesAccountId.Value, Debit = 0, Credit = invoiceTotal });
                    db.JournalVouchers.Add(journalVoucher);
                    // --- نهاية التحديث ---

                    // --- تحديث حالة أمر البيع ---
                    var orderToUpdate = db.SalesOrders.Find(_salesOrderId);
                    var totalOrdered = db.SalesOrderItems.Where(i => i.SalesOrderId == _salesOrderId).Sum(i => i.Quantity);
                    var totalShipped = db.ShipmentItems.Include(si => si.Shipment).Where(si => si.Shipment.SalesOrderId == _salesOrderId).Sum(i => i.Quantity) + _itemsToShip.Sum(i => i.QuantityToShip);
                    if (totalShipped >= totalOrdered) { orderToUpdate.Status = OrderStatus.Invoiced; }
                    else { orderToUpdate.Status = OrderStatus.PartiallyShipped; }

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم إنشاء الشحنة والفوترة والقيد المحاسبي بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
