// GoodMorningFactory/Core/Services/PurchaseService.cs
// *** الكود الكامل والشامل والنهائي ***
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
    public class PurchaseService : IPurchaseService
    {
        /// <summary>
        /// دالة جلب فواتير المشتريات مع الفلترة والترقيم.
        /// </summary>
        public async Task<PaginatedResult<PurchaseViewModel>> GetPurchasesAsync(PurchaseFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Purchases.Include(p => p.Supplier).AsQueryable();

                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string searchTextLower = criteria.SearchText.ToLower();
                    query = query.Where(p => p.InvoiceNumber.ToLower().Contains(searchTextLower) || p.Supplier.Name.ToLower().Contains(searchTextLower));
                }

                if (criteria.Status.HasValue)
                {
                    query = query.Where(p => p.Status == criteria.Status.Value);
                }

                int totalItems = await query.CountAsync();

                var purchasesForPage = await query.OrderByDescending(p => p.PurchaseDate)
                                                  .Skip((criteria.Page - 1) * criteria.PageSize)
                                                  .Take(criteria.PageSize)
                                                  .ToListAsync();

                var purchaseIds = purchasesForPage.Select(p => p.Id).ToList();
                var returnsTotals = await db.PurchaseReturns
                                              .Where(pr => purchaseIds.Contains(pr.PurchaseId))
                                              .GroupBy(pr => pr.PurchaseId)
                                              .Select(g => new { PurchaseId = g.Key, TotalReturned = g.Sum(pr => pr.TotalReturnValue) })
                                              .ToDictionaryAsync(x => x.PurchaseId, x => x.TotalReturned);

                var viewModels = purchasesForPage.Select(p => new PurchaseViewModel
                {
                    Id = p.Id,
                    InvoiceNumber = p.InvoiceNumber,
                    SupplierName = p.Supplier.Name,
                    PurchaseDate = p.PurchaseDate,
                    DueDate = p.DueDate,
                    TotalAmount = p.TotalAmount,
                    AmountPaid = p.AmountPaid,
                    Status = p.Status,
                    TotalReturned = returnsTotals.ContainsKey(p.Id) ? returnsTotals[p.Id] : 0
                }).ToList();


                return new PaginatedResult<PurchaseViewModel> { Items = viewModels, TotalCount = totalItems };
            }
        }

        /// <summary>
        /// دالة جلب فاتورة واحدة بكامل تفاصيلها لغرض الطباعة.
        /// </summary>
        public async Task<Purchase> GetPurchaseForPrintingAsync(int purchaseId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Purchases
                    .Include(p => p.Supplier)
                    .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                    .FirstOrDefaultAsync(p => p.Id == purchaseId);
            }
        }

        /// <summary>
        /// دالة جلب البيانات لنافذة الإنشاء والتعديل. تتعامل مع كل الحالات الممكنة.
        /// </summary>
        public async Task<AddEditPurchaseDto> GetDataForAddEditAsync(int? purchaseId, int? purchaseOrderId, int? grnId)
        {
            using (var db = new DatabaseContext())
            {
                var dto = new AddEditPurchaseDto
                {
                    AllSuppliers = await db.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(),
                    AllProducts = await db.Products.AsNoTracking().Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(),
                    Purchase = new Purchase()
                };

                if (purchaseId.HasValue)
                {
                    dto.Purchase = await db.Purchases.Include(p => p.PurchaseItems).AsNoTracking().FirstOrDefaultAsync(p => p.Id == purchaseId.Value);
                }
                else if (purchaseOrderId.HasValue || grnId.HasValue)
                {
                    int poId = purchaseOrderId ?? 0;
                    if (grnId.HasValue)
                    {
                        var grn = await db.GoodsReceiptNotes.FindAsync(grnId.Value);
                        if (grn != null)
                        {
                            poId = grn.PurchaseOrderId;
                            if (grn.PurchaseId == null) // التأكد من أن سند الاستلام لم يفوتر
                            {
                                dto.SourceGrnIds.Add(grn.Id);
                            }
                        }
                    }
                    else if (purchaseOrderId.HasValue)
                    {
                        var unbilledGrns = await db.GoodsReceiptNotes
                            .Where(g => g.PurchaseOrderId == purchaseOrderId.Value && g.PurchaseId == null)
                            .Select(g => g.Id)
                            .ToListAsync();
                        dto.SourceGrnIds.AddRange(unbilledGrns);
                    }

                    if (dto.SourceGrnIds.Any())
                    {
                        var po = await db.PurchaseOrders.Include(p => p.PurchaseOrderItems).FirstOrDefaultAsync(p => p.Id == poId);
                        if (po != null)
                        {
                            dto.Purchase.SupplierId = po.SupplierId;
                            dto.Purchase.PurchaseOrderId = po.Id;
                            var itemsToInvoice = await db.GoodsReceiptNoteItems
                                .Where(gri => dto.SourceGrnIds.Contains(gri.GoodsReceiptNoteId))
                                .GroupBy(gri => gri.ProductId)
                                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.QuantityReceived) })
                                .ToListAsync();

                            foreach (var item in itemsToInvoice)
                            {
                                var poItem = po.PurchaseOrderItems.FirstOrDefault(p => p.ProductId == item.ProductId);
                                dto.Purchase.PurchaseItems.Add(new PurchaseItem
                                {
                                    ProductId = item.ProductId,
                                    Quantity = item.Quantity,
                                    UnitPrice = poItem?.UnitPrice ?? 0
                                });
                            }
                        }
                    }
                }
                return dto;
            }
        }

        /// <summary>
        /// دالة حفظ الفاتورة (إنشاء أو تحديث) مع كل المنطق المتعلق بها.
        /// </summary>
        public async Task SavePurchaseAsync(Purchase purchase, List<PurchaseInvoiceItemViewModel> items, List<int> grnIdsToInvoice)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultAccountsPayableAccountId == null || companyInfo?.DefaultGoodReceiptsAccrualAccountId == null)
                        throw new Exception("يرجى تحديد حساب 'الذمم الدائنة' و 'بضاعة مستلمة غير مفوترة' الافتراضي في الإعدادات.");

                    purchase.TotalAmount = items.Sum(i => i.Subtotal);

                    if (purchase.Id == 0) { db.Purchases.Add(purchase); }
                    else
                    {
                        var existingPurchase = await db.Purchases.Include(p => p.PurchaseItems).FirstOrDefaultAsync(p => p.Id == purchase.Id);
                        if (existingPurchase != null)
                        {
                            db.Entry(existingPurchase).CurrentValues.SetValues(purchase);
                            db.PurchaseItems.RemoveRange(existingPurchase.PurchaseItems);
                        }
                    }
                    await db.SaveChangesAsync();

                    foreach (var item in items)
                    {
                        db.PurchaseItems.Add(new PurchaseItem { PurchaseId = purchase.Id, ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
                    }

                    if (grnIdsToInvoice != null && grnIdsToInvoice.Any())
                    {
                        var grnsToUpdate = await db.GoodsReceiptNotes
                                                   .Where(g => grnIdsToInvoice.Contains(g.Id))
                                                   .ToListAsync();
                        foreach (var grn in grnsToUpdate)
                        {
                            grn.PurchaseId = purchase.Id;
                        }
                    }

                    var voucher = await db.JournalVouchers.Include(v => v.JournalVoucherItems).FirstOrDefaultAsync(v => v.VoucherNumber == $"PI-{purchase.InvoiceNumber}");
                    if (voucher == null)
                    {
                        voucher = new JournalVoucher { VoucherNumber = $"PI-{purchase.InvoiceNumber}", Status = VoucherStatus.Posted };
                        db.JournalVouchers.Add(voucher);
                    }
                    var supplier = await db.Suppliers.FindAsync(purchase.SupplierId);
                    voucher.VoucherDate = purchase.PurchaseDate;
                    voucher.Description = $"إثبات فاتورة شراء من المورد: {supplier.Name} - فاتورة رقم: {purchase.InvoiceNumber}";
                    voucher.TotalDebit = purchase.TotalAmount;
                    voucher.TotalCredit = purchase.TotalAmount;
                    voucher.JournalVoucherItems.Clear();
                    voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultGoodReceiptsAccrualAccountId.Value, Debit = purchase.TotalAmount, Credit = 0, Description = "إثبات استلام بضاعة غير مفوترة" });
                    voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsPayableAccountId.Value, Debit = 0, Credit = purchase.TotalAmount, Description = "إثبات دائنية المورد" });

                    // ======================= بداية الإصلاح الرئيسي =======================
                    // الخطوة الأخيرة: تحديث حالة أمر الشراء الرئيسي بعد الفوترة
                    if (purchase.PurchaseOrderId.HasValue)
                    {
                        var poToUpdate = await db.PurchaseOrders
                                                 .Include(p => p.PurchaseOrderItems)
                                                 .FirstOrDefaultAsync(p => p.Id == purchase.PurchaseOrderId.Value);
                        if (poToUpdate != null)
                        {
                            var totalOrderedQuantity = poToUpdate.PurchaseOrderItems.Sum(i => i.Quantity);

                            var totalInvoicedQuantity = await db.Purchases
                                .Where(p => p.PurchaseOrderId == poToUpdate.Id)
                                .SelectMany(p => p.PurchaseItems)
                                .SumAsync(pi => pi.Quantity);

                            if (totalInvoicedQuantity >= totalOrderedQuantity)
                            {
                                poToUpdate.InvoicingStatus = POInvoicingStatus.FullyInvoiced;
                            }
                            else if (totalInvoicedQuantity > 0)
                            {
                                poToUpdate.InvoicingStatus = POInvoicingStatus.PartiallyInvoiced;
                            }

                            if (poToUpdate.ReceiptStatus == ReceiptStatus.FullyReceived && poToUpdate.InvoicingStatus == POInvoicingStatus.FullyInvoiced)
                            {
                                poToUpdate.Status = PurchaseOrderStatus.Invoiced;
                            }
                        }
                    }
                    // ======================== نهاية الإصلاح الرئيسي ========================

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