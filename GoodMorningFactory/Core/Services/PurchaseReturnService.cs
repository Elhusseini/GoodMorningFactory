// GoodMorningFactory/Core/Services/PurchaseReturnService.cs
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
    public class PurchaseReturnService : IPurchaseReturnService
    {
        public async Task<List<PurchaseReturn>> GetPurchaseReturnsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.PurchaseReturns.Include(pr => pr.Purchase.Supplier).OrderByDescending(pr => pr.ReturnDate).ToListAsync();
            }
        }

        public async Task<List<Purchase>> GetReturnablePurchasesAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Purchases.Include(p => p.Supplier)
                    .Where(p => p.Status != PurchaseInvoiceStatus.Cancelled && p.ReturnStatus != PurchaseReturnStatus.FullyReturned)
                    .OrderByDescending(p => p.PurchaseDate).ToListAsync();
            }
        }

        public async Task<Purchase> GetPurchaseDetailsForReturnAsync(int purchaseId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Purchases.Include(p => p.Supplier).Include(p => p.PurchaseItems).ThenInclude(pi => pi.Product).FirstOrDefaultAsync(p => p.Id == purchaseId);
            }
        }

        public async Task<Dictionary<int, int>> GetReturnedItemsForPurchaseAsync(int purchaseId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.PurchaseReturnItems
                    .Where(pri => pri.PurchaseReturn.PurchaseId == purchaseId)
                    .GroupBy(pri => pri.ProductId)
                    .Select(g => new { ProductId = g.Key, TotalReturned = g.Sum(i => i.Quantity) })
                    .ToDictionaryAsync(x => x.ProductId, x => x.TotalReturned);
            }
        }

        public async Task CreatePurchaseReturnAsync(int purchaseId, IEnumerable<PurchaseReturnItemViewModel> itemsToReturn)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultPurchaseReturnsAccountId == null || companyInfo?.DefaultAccountsPayableAccountId == null)
                        throw new Exception("يرجى تحديد حساب 'مردودات المشتريات' و 'الذمم الدائنة' الافتراضي في الإعدادات.");

                    var purchaseReturn = new PurchaseReturn { ReturnNumber = $"PRTN-{DateTime.Now:yyyyMMddHHmmss}", ReturnDate = DateTime.Today, PurchaseId = purchaseId, Status = PurchaseReturnStatus.Posted };
                    decimal totalReturnValue = 0;

                    foreach (var item in itemsToReturn)
                    {
                        var totalStock = await db.Inventories.Where(i => i.ProductId == item.ProductId).SumAsync(i => i.Quantity);
                        if (totalStock < item.QuantityToReturn) throw new Exception($"إجمالي الكمية للمنتج '{item.ProductName}' ({totalStock}) غير كافية لإرجاع الكمية المطلوبة ({item.QuantityToReturn}).");

                        var inventoryToUpdate = await db.Inventories.Where(i => i.ProductId == item.ProductId && i.Quantity >= item.QuantityToReturn).FirstOrDefaultAsync();
                        if (inventoryToUpdate == null) throw new Exception($"لا يوجد مخزن واحد يحتوي على الكمية الكافية لإرجاع المنتج '{item.ProductName}'.");

                        inventoryToUpdate.Quantity -= item.QuantityToReturn;
                        purchaseReturn.PurchaseReturnItems.Add(new PurchaseReturnItem { ProductId = item.ProductId, Quantity = item.QuantityToReturn });
                        totalReturnValue += item.QuantityToReturn * item.UnitPrice;

                        var product = await db.Products.FindAsync(item.ProductId);
                        db.StockMovements.Add(new StockMovement { ProductId = item.ProductId, StorageLocationId = inventoryToUpdate.StorageLocationId, MovementDate = purchaseReturn.ReturnDate, MovementType = StockMovementType.PurchaseReturn, Quantity = -item.QuantityToReturn, UnitCost = product.AverageCost, ReferenceDocument = purchaseReturn.ReturnNumber, UserId = CurrentUserService.LoggedInUser?.Id });
                    }

                    purchaseReturn.TotalReturnValue = totalReturnValue;
                    db.PurchaseReturns.Add(purchaseReturn);

                    var originalPurchase = await db.Purchases.Include(p => p.Supplier).Include(p => p.PurchaseItems).FirstOrDefaultAsync(p => p.Id == purchaseId);

                    var debitNoteVoucher = new JournalVoucher { VoucherNumber = $"DN-{purchaseReturn.ReturnNumber}", VoucherDate = DateTime.Today, Description = $"إشعار مدين للمورد: {originalPurchase.Supplier.Name} لمرتجع من الفاتورة رقم: {originalPurchase.InvoiceNumber}", TotalDebit = totalReturnValue, TotalCredit = totalReturnValue, Status = VoucherStatus.Posted };
                    debitNoteVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsPayableAccountId.Value, Debit = totalReturnValue, Credit = 0, Description = $"مرتجع من فاتورة {originalPurchase.InvoiceNumber}" });
                    debitNoteVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultPurchaseReturnsAccountId.Value, Debit = 0, Credit = totalReturnValue, Description = $"مرتجع للمورد {originalPurchase.Supplier.Name}" });
                    db.JournalVouchers.Add(debitNoteVoucher);

                    if (originalPurchase != null)
                    {
                        var allReturnsForPurchase = await db.PurchaseReturns.Where(pr => pr.PurchaseId == purchaseId).ToListAsync();
                        decimal newTotalReturned = allReturnsForPurchase.Sum(pr => pr.TotalReturnValue) + totalReturnValue;

                        var totalPurchasedQty = originalPurchase.PurchaseItems.Sum(pi => pi.Quantity);
                        var totalReturnedQty = allReturnsForPurchase.SelectMany(pr => pr.PurchaseReturnItems).Sum(pri => pri.Quantity) + itemsToReturn.Sum(i => i.QuantityToReturn);

                        if (totalReturnedQty >= totalPurchasedQty) { originalPurchase.ReturnStatus = PurchaseReturnStatus.FullyReturned; }
                        else { originalPurchase.ReturnStatus = PurchaseReturnStatus.PartiallyReturned; }

                        decimal balanceDue = originalPurchase.TotalAmount - originalPurchase.AmountPaid - newTotalReturned;
                        if (balanceDue <= 0) { originalPurchase.Status = PurchaseInvoiceStatus.FullyPaid; }
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception) { await transaction.RollbackAsync(); throw; }
            }
        }
    }
}