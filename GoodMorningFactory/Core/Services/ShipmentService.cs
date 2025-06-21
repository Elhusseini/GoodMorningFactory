// GoodMorningFactory/Core/Services/ShipmentService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية لإدارة العمليات المتعلقة بالشحنات.
    /// </summary>
    public class ShipmentService : IShipmentService
    {
        // --- دالة GetShipmentsAsync بدون تغيير ---
        public async Task<PaginatedResult<Shipment>> GetShipmentsAsync(ShipmentFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Shipments
                    .Include(s => s.SalesOrder)
                    .ThenInclude(so => so.Customer)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string lowerSearch = criteria.SearchText.ToLower();
                    query = query.Where(s => s.ShipmentNumber.ToLower().Contains(lowerSearch) ||
                                            s.SalesOrder.SalesOrderNumber.ToLower().Contains(lowerSearch) ||
                                            s.SalesOrder.Customer.CustomerName.ToLower().Contains(lowerSearch));
                }
                if (criteria.Status.HasValue)
                {
                    query = query.Where(s => s.Status == criteria.Status.Value);
                }
                if (criteria.FromDate.HasValue)
                {
                    query = query.Where(s => s.ShipmentDate.Date >= criteria.FromDate.Value.Date);
                }
                if (criteria.ToDate.HasValue)
                {
                    query = query.Where(s => s.ShipmentDate.Date <= criteria.ToDate.Value.Date);
                }

                var totalItems = await query.CountAsync();
                var shipments = await query.OrderByDescending(s => s.ShipmentDate)
                                            .Skip((criteria.Page - 1) * criteria.PageSize)
                                            .Take(criteria.PageSize)
                                            .ToListAsync();

                return new PaginatedResult<Shipment> { Items = shipments, TotalCount = totalItems };
            }
        }

        // --- دالة GetShipmentForEditAsync بدون تغيير ---
        public async Task<Shipment> GetShipmentForEditAsync(int shipmentId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Shipments.FindAsync(shipmentId);
            }
        }

        // --- دالة UpdateShipmentDetailsAsync بدون تغيير ---
        public async Task UpdateShipmentDetailsAsync(int shipmentId, string carrier, string trackingNumber)
        {
            using (var db = new DatabaseContext())
            {
                var shipment = await db.Shipments.FindAsync(shipmentId);
                if (shipment != null)
                {
                    shipment.Carrier = carrier;
                    shipment.TrackingNumber = trackingNumber;
                    await db.SaveChangesAsync();
                }
            }
        }

        // --- دالة GetShipmentForPackingSlipAsync بدون تغيير ---
        public async Task<Shipment> GetShipmentForPackingSlipAsync(int shipmentId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Shipments
                                .Include(s => s.SalesOrder.Customer)
                                .Include(s => s.ShipmentItems).ThenInclude(si => si.Product)
                                .FirstOrDefaultAsync(s => s.Id == shipmentId);
            }
        }

        // --- دالة GetShipmentDataForCreationAsync بدون تغيير ---
        public async Task<ShipmentCreationDataDto> GetShipmentDataForCreationAsync(int salesOrderId)
        {
            using (var db = new DatabaseContext())
            {
                var order = await db.SalesOrders.FindAsync(salesOrderId);
                if (order == null) return null;

                var dto = new ShipmentCreationDataDto
                {
                    SalesOrderNumber = order.SalesOrderNumber,
                    ShipmentDate = DateTime.Today
                };

                var orderItems = await db.SalesOrderItems.Include(i => i.Product).Where(i => i.SalesOrderId == salesOrderId).ToListAsync();
                var shippedItems = await db.ShipmentItems.Include(si => si.Shipment)
                                         .Where(si => si.Shipment.SalesOrderId == salesOrderId)
                                         .GroupBy(si => si.ProductId)
                                         .ToDictionaryAsync(g => g.Key, g => g.Sum(i => i.Quantity));

                foreach (var item in orderItems)
                {
                    int previouslyShipped = shippedItems.ContainsKey(item.ProductId) ? shippedItems[item.ProductId] : 0;
                    int remaining = item.Quantity - previouslyShipped;

                    var availableStock = await db.Inventories
                        .Include(i => i.StorageLocation)
                        .Where(i => i.ProductId == item.ProductId && i.Quantity > 0)
                        .Select(i => new AvailableStockLocation
                        {
                            StorageLocationId = i.StorageLocationId,
                            LocationName = i.StorageLocation.Name,
                            QuantityOnHand = i.Quantity
                        }).ToListAsync();

                    dto.ItemsToShip.Add(new ShipmentItemViewModel
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
                return dto;
            }
        }

        // --- بداية التعديل: تحسين منطق إنشاء الشحنة والفوترة ---
        /// <summary>
        /// ينشئ شحنة وفاتورة وقيود محاسبية مرتبطة بها، ويقوم بتحديث حالة أمر البيع والمخزون.
        /// كل هذه العمليات تتم داخل عملية (Transaction) واحدة لضمان تكامل البيانات.
        /// </summary>
        public async Task CreateShipmentAndInvoiceAsync(int salesOrderId, DateTime shipmentDate, IEnumerable<ShipmentItemViewModel> itemsToShip)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultSalesAccountId == null || companyInfo?.DefaultAccountsReceivableAccountId == null ||
                        companyInfo?.DefaultInventoryAccountId == null || companyInfo?.DefaultCogsAccountId == null ||
                        companyInfo.DefaultVatAccountId == null)
                    {
                        throw new Exception("يرجى تحديد جميع الحسابات الافتراضية المطلوبة في الإعدادات.");
                    }

                    var itemsToProcess = itemsToShip.Where(i => i.QuantityToShip > 0).ToList();
                    if (!itemsToProcess.Any())
                    {
                        throw new InvalidOperationException("لم يتم تحديد أي كميات للشحن.");
                    }

                    // 1. إنشاء الشحنة
                    var shipment = new Shipment
                    {
                        ShipmentNumber = $"SHP-{DateTime.Now:yyyyMMddHHmmss}",
                        ShipmentDate = shipmentDate,
                        SalesOrderId = salesOrderId,
                        Status = ShipmentStatus.Shipped
                    };

                    decimal subtotal = 0, totalTax = 0, cogsTotal = 0;
                    var itemsToInvoice = new List<SaleItem>();

                    foreach (var item in itemsToProcess)
                    {
                        // التحقق من صحة البيانات قبل المتابعة
                        if (!item.SourceLocationId.HasValue)
                            throw new InvalidOperationException($"يجب تحديد الموقع المصدر للمنتج: {item.ProductName}");

                        var product = await db.Products.Include(p => p.TaxRule).FirstOrDefaultAsync(p => p.Id == item.ProductId);
                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.StorageLocationId == item.SourceLocationId.Value);

                        if (inventory == null || inventory.Quantity < item.QuantityToShip)
                            throw new Exception($"الكمية غير كافية في الموقع المحدد للمنتج: {item.ProductName}");

                        // 2. تحديث المخزون وإنشاء حركة مخزنية
                        inventory.Quantity -= item.QuantityToShip;
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = item.ProductId,
                            StorageLocationId = item.SourceLocationId.Value,
                            MovementDate = shipment.ShipmentDate,
                            MovementType = StockMovementType.SalesShipment,
                            Quantity = -item.QuantityToShip, // الكمية بالسالب لأنها خارجة من المخزن
                            UnitCost = product.AverageCost,
                            ReferenceDocument = shipment.ShipmentNumber,
                            UserId = CurrentUserService.LoggedInUser?.Id
                        });

                        // 3. إضافة البنود إلى الشحنة وحساب الإجماليات للفاتورة
                        shipment.ShipmentItems.Add(new ShipmentItem { ProductId = item.ProductId, Quantity = item.QuantityToShip });
                        itemsToInvoice.Add(new SaleItem { ProductId = item.ProductId, Quantity = item.QuantityToShip, UnitPrice = item.UnitPrice });

                        decimal itemSubtotal = item.QuantityToShip * item.UnitPrice;
                        decimal itemTax = 0;
                        if (product.TaxRule != null && product.TaxRule.Rate > 0)
                        {
                            itemTax = itemSubtotal * (product.TaxRule.Rate / 100);
                        }
                        subtotal += itemSubtotal;
                        totalTax += itemTax;
                        cogsTotal += InventoryCostingService.GetCostOfGoodsSold(db, item.ProductId, item.QuantityToShip);

                        // 4. تحديث حالة الأرقام التسلسلية إذا كانت موجودة
                        if (item.IsTracked && item.TrackingMethod == ProductTrackingMethod.BySerialNumber)
                        {
                            if (item.SelectedSerialIds.Count != item.QuantityToShip)
                                throw new Exception($"لم يتم اختيار الأرقام التسلسلية بشكل صحيح للمنتج: {item.ProductName}");

                            var serialsToUpdate = await db.SerialNumbers.Where(sn => item.SelectedSerialIds.Contains(sn.Id)).ToListAsync();
                            foreach (var serial in serialsToUpdate) { serial.Status = SerialNumberStatus.Shipped; }
                        }
                    }

                    db.Shipments.Add(shipment);
                    await db.SaveChangesAsync(); // حفظ الشحنة للحصول على ID

                    // 5. إنشاء الفاتورة
                    var orderToUpdate = await db.SalesOrders.Include(o => o.Customer).Include(o => o.SalesOrderItems).FirstOrDefaultAsync(o => o.Id == salesOrderId);
                    var invoice = new Sale
                    {
                        InvoiceNumber = $"INV-{shipment.ShipmentNumber}",
                        SaleDate = shipment.ShipmentDate,
                        DueDate = shipment.ShipmentDate.AddDays(30), // افتراضي 30 يوم
                        SalesOrderId = salesOrderId,
                        CustomerId = orderToUpdate.CustomerId,
                        Status = InvoiceStatus.Sent,
                        Subtotal = subtotal,
                        TaxAmount = totalTax,
                        TotalAmount = subtotal + totalTax,
                        SaleItems = itemsToInvoice
                    };
                    db.Sales.Add(invoice);
                    await db.SaveChangesAsync(); // حفظ الفاتورة للحصول على ID

                    // 6. إنشاء قيد اليومية
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

                    // 7. تحديث حالة أمر البيع
                    var totalOrderedQuantity = orderToUpdate.SalesOrderItems.Sum(i => i.Quantity);
                    var totalShippedQuantity = await db.ShipmentItems.Where(si => si.Shipment.SalesOrderId == salesOrderId).SumAsync(si => (int?)si.Quantity) ?? 0;
                    var totalInvoicedAmount = await db.Sales.Where(s => s.SalesOrderId == salesOrderId).SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

                    if (totalShippedQuantity >= totalOrderedQuantity) { orderToUpdate.ShippingStatus = ShippingStatus.FullyShipped; }
                    else if (totalShippedQuantity > 0) { orderToUpdate.ShippingStatus = ShippingStatus.PartiallyShipped; }

                    if (totalInvoicedAmount >= orderToUpdate.TotalAmount) { orderToUpdate.InvoicingStatus = InvoicingStatus.FullyInvoiced; }
                    else if (totalInvoicedAmount > 0) { orderToUpdate.InvoicingStatus = InvoicingStatus.PartiallyInvoiced; }

                    if (orderToUpdate.ShippingStatus == ShippingStatus.FullyShipped) { orderToUpdate.Status = OrderStatus.Shipped; }
                    else { orderToUpdate.Status = OrderStatus.PartiallyShipped; }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync(); // تأكيد كل التغييرات
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(); // التراجع عن كل شيء في حال حدوث خطأ
                    throw;
                }
            }
        }
        // --- نهاية التعديل ---
    }
}