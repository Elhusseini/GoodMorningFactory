// GoodMorningFactory/Core/Services/SalesOrderService.cs
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
    public class SalesOrderService : ISalesOrderService
    {
        // --- دالة GetSalesOrdersAsync بدون تغيير ---
        public async Task<PaginatedResult<SalesOrder>> GetSalesOrdersAsync(int page, int pageSize, string searchText, OrderStatus? status)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.SalesOrders.Include(o => o.Customer).AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string lowerSearch = searchText.ToLower();
                    query = query.Where(o => o.SalesOrderNumber.ToLower().Contains(lowerSearch) || o.Customer.CustomerName.ToLower().Contains(lowerSearch));
                }
                if (status.HasValue)
                {
                    query = query.Where(o => o.Status == status.Value);
                }

                var totalItems = await query.CountAsync();
                var ordersForPage = await query.OrderByDescending(o => o.OrderDate)
                                                .Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();
                return new PaginatedResult<SalesOrder> { Items = ordersForPage, TotalCount = totalItems };
            }
        }

        // --- دالة CancelSalesOrderAsync بدون تغيير ---
        public async Task CancelSalesOrderAsync(int orderId)
        {
            using (var db = new DatabaseContext())
            {
                var order = await db.SalesOrders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Cancelled;
                    await db.SaveChangesAsync();
                }
            }
        }

        // --- بداية التعديل: تحسين منطق جلب البنود لإنشاء أوامر عمل ---
        /// <summary>
        /// يجلب قائمة ببنود أمر البيع التي تحتاج إلى تصنيع (منتجات نهائية)
        /// ولم يتم إنشاء أمر عمل لها من قبل.
        /// </summary>
        public async Task<List<SalesOrderItem>> GetItemsForWorkOrderCreationAsync(int orderId)
        {
            using (var db = new DatabaseContext())
            {
                // 1. جلب كل بنود أمر البيع التي هي منتجات نهائية
                var allManufacturableItems = await db.SalesOrderItems
                    .Include(i => i.Product)
                    .Where(i => i.SalesOrderId == orderId && i.Product.ProductType == ProductType.FinishedGood)
                    .ToListAsync();

                // 2. جلب كل أوامر العمل المرتبطة بهذا أمر البيع
                var existingWorkOrders = await db.WorkOrders
                    .Where(wo => wo.SalesOrderItem.SalesOrderId == orderId)
                    .ToListAsync();

                // 3. فلترة البنود لإرجاع فقط تلك التي ليس لها أمر عمل مرتبط
                var itemsWithoutWorkOrder = allManufacturableItems
                    .Where(item => !existingWorkOrders.Any(wo => wo.SalesOrderItemId == item.Id))
                    .ToList();

                if (!itemsWithoutWorkOrder.Any())
                {
                    throw new InvalidOperationException("تم إنشاء أوامر عمل لجميع المنتجات النهائية في أمر البيع هذا بالفعل.");
                }

                return itemsWithoutWorkOrder;
            }
        }
        // --- نهاية التعديل ---

        // --- دالة UpdateOrderStatusToInProcessAsync بدون تغيير ---
        public async Task UpdateOrderStatusToInProcessAsync(int orderId)
        {
            using (var db = new DatabaseContext())
            {
                var orderToUpdate = await db.SalesOrders.FindAsync(orderId);
                if (orderToUpdate != null)
                {
                    orderToUpdate.Status = OrderStatus.InProcess;
                    await db.SaveChangesAsync();
                }
            }
        }

        // --- بقية الدوال في الخدمة بدون تغيير ---
        public async Task<SalesOrder> GetSalesOrderForEditAsync(int orderId)
        {
            using (var db = new DatabaseContext()) { return await db.SalesOrders.Include(o => o.SalesOrderItems).ThenInclude(i => i.Product).FirstOrDefaultAsync(o => o.Id == orderId); }
        }

        public async Task<SalesQuotation> GetQuotationForConversionAsync(int quotationId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.SalesQuotations
                                 .Include(q => q.Customer)
                                 .Include(q => q.SalesQuotationItems)
                                 .ThenInclude(item => item.Product)
                                 .FirstOrDefaultAsync(q => q.Id == quotationId);
            }
        }

        public async Task<Product> FindProductAsync(string searchText)
        {
            using (var db = new DatabaseContext()) { string lowerSearch = searchText.ToLower(); return await db.Products.FirstOrDefaultAsync(p => p.ProductCode.ToLower() == lowerSearch || p.Name.ToLower().Contains(lowerSearch)); }
        }

        public async Task<decimal> GetProductPriceAsync(int productId, int? priceListId)
        {
            using (var db = new DatabaseContext())
            {
                if (priceListId.HasValue)
                {
                    var productPrice = await db.ProductPrices.FirstOrDefaultAsync(pp => pp.PriceListId == priceListId.Value && pp.ProductId == productId);
                    if (productPrice != null) { return productPrice.Price; }
                }
                var product = await db.Products.FindAsync(productId);
                return product?.SalePrice ?? 0;
            }
        }

        public async Task SaveSalesOrderAsync(SalesOrder order)
        {
            using (var db = new DatabaseContext())
            {
                if (order.Id > 0)
                {
                    var existingOrder = await db.SalesOrders.Include(o => o.SalesOrderItems).FirstOrDefaultAsync(o => o.Id == order.Id);
                    db.SalesOrderItems.RemoveRange(existingOrder.SalesOrderItems);
                    db.Entry(existingOrder).CurrentValues.SetValues(order);
                    foreach (var item in order.SalesOrderItems) { existingOrder.SalesOrderItems.Add(item); }
                }
                else { db.SalesOrders.Add(order); }
                await db.SaveChangesAsync();
            }
        }

        public async Task<string> GetNextSalesOrderNumberAsync()
        {
            return $"SO-{DateTime.Now:yyyyMMddHHmmss}";
        }

        public async Task<(bool Exceeded, string Message)> CheckCreditLimitAsync(int customerId, decimal newOrderValue, int? currentOrderId)
        {
            using (var db = new DatabaseContext())
            {
                var customer = await db.Customers.FindAsync(customerId);
                if (customer == null || customer.CreditLimit <= 0) { return (false, string.Empty); }
                decimal currentBalance = await db.Sales.Where(s => s.SalesOrder.CustomerId == customerId && s.Status != InvoiceStatus.Paid && s.Status != InvoiceStatus.Cancelled && s.SalesOrderId != currentOrderId).SumAsync(s => (decimal?)(s.TotalAmount - s.AmountPaid)) ?? 0;
                if ((currentBalance + newOrderValue) > customer.CreditLimit)
                {
                    string message = $"قيمة هذا الأمر ستؤدي إلى تجاوز حد الائتمان للعميل '{customer.CustomerName}'.\n" + $"الرصيد الحالي: {currentBalance:N2}\n" + $"قيمة الأمر الجديد: {newOrderValue:N2}\n" + $"الإجمالي: {(currentBalance + newOrderValue):N2}\n" + $"حد الائتمان: {customer.CreditLimit:N2}";
                    return (true, message);
                }
                return (false, string.Empty);
            }
        }

        public async Task<InvoiceCreationDataDto> GetDataForInvoicingAsync(int salesOrderId)
        {
            using (var db = new DatabaseContext())
            {
                var order = await db.SalesOrders.Include(o => o.Customer).FirstOrDefaultAsync(o => o.Id == salesOrderId);
                if (order == null) return null;

                var dto = new InvoiceCreationDataDto
                {
                    SalesOrderNumber = order.SalesOrderNumber,
                    PaymentTerms = 30 // Assuming default
                };

                var invoicedItems = await db.SaleItems
                                              .Where(si => si.Sale.SalesOrderId == salesOrderId)
                                              .GroupBy(si => si.ProductId)
                                              .Select(g => new { ProductId = g.Key, TotalInvoiced = g.Sum(i => i.Quantity) })
                                              .ToDictionaryAsync(g => g.ProductId, g => g.TotalInvoiced);

                var orderItems = await db.SalesOrderItems
                                          .Include(oi => oi.Product)
                                          .Where(oi => oi.SalesOrderId == salesOrderId)
                                          .ToListAsync();

                foreach (var item in orderItems)
                {
                    int previouslyInvoiced = invoicedItems.GetValueOrDefault(item.ProductId);
                    if (item.Quantity > previouslyInvoiced)
                    {
                        dto.InvoiceableItems.Add(new InvoiceItemViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Product.Name,
                            OrderedQuantity = item.Quantity,
                            InvoicedQuantity = previouslyInvoiced,
                            QuantityToInvoice = item.Quantity - previouslyInvoiced,
                            UnitPrice = item.UnitPrice
                        });
                    }
                }
                return dto;
            }
        }

        public async Task CreateInvoiceFromOrderAsync(int salesOrderId, DateTime invoiceDate, DateTime? dueDate, IEnumerable<InvoiceItemViewModel> itemsToInvoice)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var itemsToProcess = itemsToInvoice.Where(i => i.QuantityToInvoice > 0).ToList();
                    if (!itemsToProcess.Any()) throw new InvalidOperationException("لم يتم تحديد أي كميات للفوترة.");

                    var order = await db.SalesOrders.Include(o => o.Customer).Include(o => o.SalesOrderItems).FirstOrDefaultAsync(o => o.Id == salesOrderId);
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();

                    decimal subtotal = 0, totalTax = 0, cogsTotal = 0;
                    var saleItems = new List<SaleItem>();

                    foreach (var item in itemsToProcess)
                    {
                        var product = await db.Products.Include(p => p.TaxRule).FirstOrDefaultAsync(p => p.Id == item.ProductId);
                        decimal itemSubtotal = item.QuantityToInvoice * item.UnitPrice;
                        decimal itemTax = 0;
                        if (product.TaxRule != null && product.TaxRule.Rate > 0) { itemTax = itemSubtotal * (product.TaxRule.Rate / 100); }
                        subtotal += itemSubtotal;
                        totalTax += itemTax;

                        cogsTotal += InventoryCostingService.GetCostOfGoodsSold(db, item.ProductId, item.QuantityToInvoice);
                        saleItems.Add(new SaleItem { ProductId = item.ProductId, Quantity = item.QuantityToInvoice, UnitPrice = item.UnitPrice });
                    }

                    var invoice = new Sale
                    {
                        InvoiceNumber = $"INV-SO-{order.SalesOrderNumber}-{DateTime.Now:mmss}",
                        SaleDate = invoiceDate,
                        DueDate = dueDate,
                        SalesOrderId = salesOrderId,
                        CustomerId = order.CustomerId,
                        Status = InvoiceStatus.Sent,
                        Subtotal = subtotal,
                        TaxAmount = totalTax,
                        TotalAmount = subtotal + totalTax,
                        SaleItems = saleItems
                    };
                    db.Sales.Add(invoice);
                    await db.SaveChangesAsync();

                    var jv = new JournalVoucher
                    {
                        VoucherNumber = $"JV-{invoice.InvoiceNumber}",
                        VoucherDate = invoice.SaleDate,
                        Description = $"قيد فاتورة من أمر البيع {order.SalesOrderNumber}",
                        TotalDebit = invoice.TotalAmount + cogsTotal,
                        TotalCredit = invoice.TotalAmount + cogsTotal,
                        Status = VoucherStatus.Posted
                    };
                    jv.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsReceivableAccountId.Value, Debit = invoice.TotalAmount });
                    jv.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultCogsAccountId.Value, Debit = cogsTotal });
                    jv.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultSalesAccountId.Value, Credit = invoice.Subtotal });
                    jv.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultVatAccountId.Value, Credit = invoice.TaxAmount });
                    jv.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Credit = cogsTotal });
                    db.JournalVouchers.Add(jv);

                    var totalInvoicedAmount = await db.Sales.Where(s => s.SalesOrderId == salesOrderId).SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
                    if (totalInvoicedAmount >= order.TotalAmount) { order.InvoicingStatus = InvoicingStatus.FullyInvoiced; }
                    else if (totalInvoicedAmount > 0) { order.InvoicingStatus = InvoicingStatus.PartiallyInvoiced; }
                    if (order.ShippingStatus == ShippingStatus.FullyShipped && order.InvoicingStatus == InvoicingStatus.FullyInvoiced) { order.Status = OrderStatus.Invoiced; }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}