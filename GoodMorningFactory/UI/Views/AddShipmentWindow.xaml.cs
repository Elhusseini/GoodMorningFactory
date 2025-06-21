// UI/Views/AddShipmentWindow.xaml.cs
// *** تحديث: استخدام خدمة حساب التكلفة المركزية لحساب تكلفة البضاعة المباعة ***
using GoodMorningFactory.Core.Services; // <-- إضافة مهمة
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddShipmentWindow : Window
    {
        private readonly int _salesOrderId;
        private List<ShipmentItemViewModel> _itemsToShip = new List<ShipmentItemViewModel>();

        public AddShipmentWindow(int salesOrderId)
        {
            InitializeComponent();
            _salesOrderId = salesOrderId;
            ShipmentItemsDataGrid.ItemsSource = _itemsToShip;
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

                    var orderItems = db.SalesOrderItems.Include(i => i.Product).Where(i => i.SalesOrderId == _salesOrderId).ToList();
                    var shippedItems = db.ShipmentItems.Include(si => si.Shipment)
                                         .Where(si => si.Shipment.SalesOrderId == _salesOrderId)
                                         .GroupBy(si => si.ProductId)
                                         .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

                    foreach (var item in orderItems)
                    {
                        int previouslyShipped = shippedItems.ContainsKey(item.ProductId) ? shippedItems[item.ProductId] : 0;
                        int remaining = item.Quantity - previouslyShipped;

                        var availableStock = db.Inventories
                            .Include(i => i.StorageLocation)
                            .Where(i => i.ProductId == item.ProductId && i.Quantity > 0)
                            .Select(i => new AvailableStockLocation
                            {
                                StorageLocationId = i.StorageLocationId,
                                LocationName = i.StorageLocation.Name,
                                QuantityOnHand = i.Quantity
                            }).ToList();

                        _itemsToShip.Add(new ShipmentItemViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Product.Name,
                            OrderedQuantity = item.Quantity,
                            PreviouslyShippedQuantity = previouslyShipped,
                            QuantityToShip = remaining > 0 ? remaining : 0,
                            UnitPrice = item.UnitPrice,
                            AvailableLocations = availableStock,
                            SourceLocationId = availableStock.FirstOrDefault()?.StorageLocationId,
                            IsTracked = item.Product.TrackingMethod != ProductTrackingMethod.None,
                            TrackingMethod = item.Product.TrackingMethod
                        });
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل بيانات الأمر: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void SelectTrackingDataButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.Button)?.DataContext is ShipmentItemViewModel item)
            {
                if (item.QuantityToShip <= 0)
                {
                    MessageBox.Show("يرجى تحديد الكمية المراد شحنها أولاً.", "تنبيه");
                    return;
                }
                if (!item.SourceLocationId.HasValue)
                {
                    MessageBox.Show("يرجى تحديد الموقع المصدر أولاً.", "تنبيه");
                    return;
                }

                var selectionWindow = new SelectTrackingDataWindow(item.ProductId, item.SourceLocationId.Value, item.QuantityToShip, item.TrackingMethod);
                if (selectionWindow.ShowDialog() == true)
                {
                    item.SelectedSerialIds = selectionWindow.SelectedIds;
                    MessageBox.Show($"تم اختيار {item.SelectedSerialIds.Count} رقم بنجاح.", "نجاح");
                }
            }
        }

        private void ConfirmShipmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_itemsToShip.Any(item => item.QuantityToShip > 0 && item.SourceLocationId == null))
            {
                MessageBox.Show("يرجى تحديد الموقع المصدر لكل صنف سيتم شحنه.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var companyInfo = db.CompanyInfos.FirstOrDefault();
                    if (companyInfo?.DefaultSalesAccountId == null || companyInfo?.DefaultAccountsReceivableAccountId == null ||
                        companyInfo?.DefaultInventoryAccountId == null || companyInfo?.DefaultCogsAccountId == null ||
                        companyInfo.DefaultVatAccountId == null)
                    {
                        throw new Exception("يرجى تحديد جميع الحسابات الافتراضية المطلوبة (المبيعات، الذمم، المخزون، التكلفة، والضريبة) في شاشة الإعدادات أولاً.");
                    }

                    var shipment = new Shipment
                    {
                        ShipmentNumber = $"SHP-{DateTime.Now:yyyyMMddHHmmss}",
                        ShipmentDate = ShipmentDatePicker.SelectedDate ?? DateTime.Today,
                        SalesOrderId = _salesOrderId,
                        Status = ShipmentStatus.Shipped
                    };

                    bool somethingWasShipped = false;
                    decimal subtotal = 0;
                    decimal totalTax = 0;
                    decimal cogsTotal = 0;
                    var itemsToInvoice = new List<SaleItem>();

                    foreach (var item in _itemsToShip)
                    {
                        if (item.QuantityToShip > 0)
                        {
                            somethingWasShipped = true;
                            shipment.ShipmentItems.Add(new ShipmentItem { ProductId = item.ProductId, Quantity = item.QuantityToShip });

                            var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId && i.StorageLocationId == item.SourceLocationId.Value);
                            var product = db.Products.Include(p => p.TaxRule).FirstOrDefault(p => p.Id == item.ProductId);

                            if (inventory == null || inventory.Quantity < item.QuantityToShip)
                            {
                                throw new Exception($"الكمية غير كافية في الموقع المحدد للمنتج: {item.ProductName}");
                            }

                            inventory.Quantity -= item.QuantityToShip;

                            db.StockMovements.Add(new StockMovement
                            {
                                ProductId = item.ProductId,
                                StorageLocationId = item.SourceLocationId.Value,
                                MovementDate = shipment.ShipmentDate,
                                MovementType = StockMovementType.SalesShipment,
                                Quantity = item.QuantityToShip,
                                UnitCost = product.AverageCost,
                                ReferenceDocument = shipment.ShipmentNumber,
                                UserId = CurrentUserService.LoggedInUser?.Id
                            });

                            decimal itemSubtotal = item.QuantityToShip * item.UnitPrice;
                            decimal itemTax = 0;

                            if (product.TaxRule != null && product.TaxRule.Rate > 0)
                            {
                                itemTax = itemSubtotal * (product.TaxRule.Rate / 100);
                            }

                            subtotal += itemSubtotal;
                            totalTax += itemTax;

                            itemsToInvoice.Add(new SaleItem { ProductId = item.ProductId, Quantity = item.QuantityToShip, UnitPrice = item.UnitPrice });

                            // --- بداية التحديث: استخدام الخدمة الجديدة لحساب التكلفة ---
                            cogsTotal += InventoryCostingService.GetCostOfGoodsSold(db, item.ProductId, item.QuantityToShip);
                            // --- نهاية التحديث ---

                            if (item.IsTracked)
                            {
                                if (item.TrackingMethod == ProductTrackingMethod.BySerialNumber)
                                {
                                    if (item.SelectedSerialIds.Count != item.QuantityToShip)
                                    {
                                        throw new Exception($"لم يتم اختيار الأرقام التسلسلية بشكل صحيح للمنتج: {item.ProductName}");
                                    }

                                    var serialsToUpdate = db.SerialNumbers.Where(sn => item.SelectedSerialIds.Contains(sn.Id)).ToList();
                                    foreach (var serial in serialsToUpdate)
                                    {
                                        serial.Status = SerialNumberStatus.Shipped;
                                    }
                                }
                            }
                        }
                    }

                    if (!somethingWasShipped) { MessageBox.Show("لم يتم تحديد أي كميات للشحن.", "تنبيه"); return; }
                    db.Shipments.Add(shipment);

                    var orderToUpdate = db.SalesOrders.Include(o => o.Customer).Include(o => o.SalesOrderItems).FirstOrDefault(o => o.Id == _salesOrderId);
                    if (orderToUpdate == null) throw new Exception("لم يتم العثور على أمر البيع المرتبط.");

                    var invoice = new Sale
                    {
                        InvoiceNumber = $"INV-{shipment.ShipmentNumber}",
                        SaleDate = shipment.ShipmentDate,
                        DueDate = shipment.ShipmentDate.AddDays(30),
                        SalesOrderId = _salesOrderId,
                        CustomerId = orderToUpdate.CustomerId,
                        Status = InvoiceStatus.Sent,
                        Subtotal = subtotal,
                        TaxAmount = totalTax,
                        TotalAmount = subtotal + totalTax,
                        AmountPaid = 0,
                        SaleItems = itemsToInvoice
                    };
                    db.Sales.Add(invoice);

                    var salesJournalVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"JV-{invoice.InvoiceNumber}",
                        VoucherDate = invoice.SaleDate,
                        Description = $"قيد إثبات مبيعات وتكلفة الفاتورة رقم {invoice.InvoiceNumber}",
                        TotalDebit = invoice.TotalAmount + cogsTotal,
                        TotalCredit = invoice.TotalAmount + cogsTotal,
                        Status = VoucherStatus.Posted
                    };

                    salesJournalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsReceivableAccountId.Value, Debit = invoice.TotalAmount, Credit = 0 });
                    salesJournalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultCogsAccountId.Value, Debit = cogsTotal, Credit = 0 });
                    salesJournalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultSalesAccountId.Value, Debit = 0, Credit = invoice.Subtotal });
                    salesJournalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultVatAccountId.Value, Debit = 0, Credit = invoice.TaxAmount });
                    salesJournalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = 0, Credit = cogsTotal });
                    db.JournalVouchers.Add(salesJournalVoucher);

                    var totalOrderedQuantity = orderToUpdate.SalesOrderItems.Sum(i => i.Quantity);
                    var totalShippedQuantity = db.ShipmentItems.Where(si => si.Shipment.SalesOrderId == _salesOrderId).Sum(si => (int?)si.Quantity) ?? 0;
                    var totalInvoicedAmount = db.Sales.Where(s => s.SalesOrderId == _salesOrderId).Sum(s => (decimal?)s.TotalAmount) ?? 0;

                    if (totalShippedQuantity >= totalOrderedQuantity) { orderToUpdate.ShippingStatus = ShippingStatus.FullyShipped; }
                    else if (totalShippedQuantity > 0) { orderToUpdate.ShippingStatus = ShippingStatus.PartiallyShipped; }

                    if (totalInvoicedAmount >= orderToUpdate.TotalAmount) { orderToUpdate.InvoicingStatus = InvoicingStatus.FullyInvoiced; }
                    else if (totalInvoicedAmount > 0) { orderToUpdate.InvoicingStatus = InvoicingStatus.PartiallyInvoiced; }

                    if (orderToUpdate.ShippingStatus == ShippingStatus.FullyShipped && orderToUpdate.InvoicingStatus == InvoicingStatus.FullyInvoiced) { orderToUpdate.Status = OrderStatus.Invoiced; }
                    else { orderToUpdate.Status = OrderStatus.PartiallyShipped; }

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم إنشاء الشحنة وتحديث بيانات التتبع بنجاح.", "نجاح");
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
