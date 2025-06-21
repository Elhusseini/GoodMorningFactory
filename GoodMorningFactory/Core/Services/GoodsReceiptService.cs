// GoodMorningFactory/Core/Services/GoodsReceiptService.cs
// *** الكود الكامل والنهائي بعد إعادة كتابة منطق الحفظ بالكامل ***
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
    public class GoodsReceiptService : IGoodsReceiptService
    {
        public async Task<PagedResult<GoodsReceiptNote>> GetPagedGoodsReceiptsAsync(int page, int pageSize)
        {
            using (var db = new DatabaseContext())
            {
                var totalItems = await db.GoodsReceiptNotes.CountAsync();
                var items = await db.GoodsReceiptNotes
                    .Include(grn => grn.PurchaseOrder.Supplier)
                    .OrderByDescending(grn => grn.ReceiptDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<GoodsReceiptNote> { Items = items, TotalItems = totalItems };
            }
        }

        public async Task<List<GoodsReceiptItemViewModel>> GetDataForReceiptCreationAsync(int purchaseOrderId)
        {
            using (var db = new DatabaseContext())
            {
                var itemsToReceive = new List<GoodsReceiptItemViewModel>();
                var allLocations = await db.StorageLocations.Where(l => l.IsActive).ToListAsync();
                var orderItems = await db.PurchaseOrderItems.Include(i => i.Product).Where(i => i.PurchaseOrderId == purchaseOrderId).ToListAsync();

                var receivedItems = await db.GoodsReceiptNotes
                    .Where(grn => grn.PurchaseOrderId == purchaseOrderId)
                    .SelectMany(grn => grn.GoodsReceiptNoteItems)
                    .GroupBy(gri => gri.ProductId)
                    .ToDictionaryAsync(g => g.Key, g => g.Sum(i => i.QuantityReceived));

                foreach (var item in orderItems)
                {
                    int previouslyReceived = receivedItems.ContainsKey(item.ProductId) ? receivedItems[item.ProductId] : 0;
                    int remaining = item.Quantity - previouslyReceived;

                    if (remaining > 0)
                    {
                        itemsToReceive.Add(new GoodsReceiptItemViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Product.Name,
                            OrderedQuantity = item.Quantity,
                            PreviouslyReceivedQuantity = previouslyReceived,
                            QuantityReceived = remaining,
                            UnitPrice = item.UnitPrice,
                            AvailableLocations = allLocations,
                            DestinationLocationId = allLocations.FirstOrDefault(l => l.IsDefault)?.Id ?? allLocations.FirstOrDefault()?.Id,
                            IsTracked = item.Product.TrackingMethod != ProductTrackingMethod.None,
                            TrackingMethod = item.Product.TrackingMethod
                        });
                    }
                }
                return itemsToReceive;
            }
        }

        public async Task SaveGoodsReceiptAsync(int purchaseOrderId, IEnumerable<GoodsReceiptItemViewModel> items)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultInventoryAccountId == null || companyInfo?.DefaultGoodReceiptsAccrualAccountId == null)
                        throw new Exception("يرجى تحديد حساب 'المخزون' و 'بضاعة مستلمة غير مفوترة' الافتراضي في شاشة الإعدادات.");

                    var itemsToProcess = items.Where(i => i.QuantityReceived > 0).ToList();
                    if (!itemsToProcess.Any())
                        throw new InvalidOperationException("لم يتم تحديد أي كميات للاستلام.");

                    var grn = new GoodsReceiptNote
                    {
                        GRNNumber = $"GRN-{DateTime.Now:yyyyMMddHHmmss}",
                        ReceiptDate = DateTime.Today,
                        PurchaseOrderId = purchaseOrderId
                    };
                    db.GoodsReceiptNotes.Add(grn);

                    decimal totalReceiptValue = 0;

                    foreach (var itemVM in itemsToProcess)
                    {
                        if (itemVM.DestinationLocationId == null) throw new Exception($"يجب تحديد موقع التخزين للمنتج: {itemVM.ProductName}");

                        var product = await db.Products.FindAsync(itemVM.ProductId);
                        if (product == null) continue;

                        grn.GoodsReceiptNoteItems.Add(new GoodsReceiptNoteItem
                        {
                            ProductId = itemVM.ProductId,
                            QuantityReceived = itemVM.QuantityReceived
                        });

                        totalReceiptValue += itemVM.QuantityReceived * itemVM.UnitPrice;

                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == itemVM.ProductId && i.StorageLocationId == itemVM.DestinationLocationId.Value);
                        if (inventory != null)
                        {
                            inventory.Quantity += itemVM.QuantityReceived;
                        }
                        else
                        {
                            db.Inventories.Add(new Inventory { ProductId = itemVM.ProductId, StorageLocationId = itemVM.DestinationLocationId.Value, Quantity = itemVM.QuantityReceived, LastUpdated = DateTime.Now });
                        }

                        // إنشاء حركة مخزنية
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = itemVM.ProductId,
                            StorageLocationId = itemVM.DestinationLocationId.Value,
                            MovementDate = grn.ReceiptDate,
                            MovementType = StockMovementType.PurchaseReceipt,
                            Quantity = itemVM.QuantityReceived,
                            UnitCost = itemVM.UnitPrice,
                            ReferenceDocument = grn.GRNNumber,
                            UserId = CurrentUserService.LoggedInUser?.Id
                        });
                    }

                    // إنشاء القيد المحاسبي
                    if (totalReceiptValue > 0)
                    {
                        var voucher = new JournalVoucher
                        {
                            VoucherNumber = $"JV-GRN-{grn.GRNNumber}",
                            VoucherDate = DateTime.Today,
                            Description = $"قيد استلام بضاعة للمذكرة رقم: {grn.GRNNumber}",
                            TotalDebit = totalReceiptValue,
                            TotalCredit = totalReceiptValue,
                            Status = VoucherStatus.Posted
                        };
                        voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = totalReceiptValue, Credit = 0, Description = "إثبات زيادة المخزون" });
                        voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultGoodReceiptsAccrualAccountId.Value, Debit = 0, Credit = totalReceiptValue, Description = "إثبات التزام استلام بضاعة غير مفوترة" });
                        db.JournalVouchers.Add(voucher);
                    }

                    // ======================= بداية الإصلاح الرئيسي =======================
                    // الخطوة الأخيرة والمهمة: تحديث حالة أمر الشراء الرئيسي
                    var poToUpdate = await db.PurchaseOrders
                                             .Include(p => p.PurchaseOrderItems)
                                             .FirstOrDefaultAsync(p => p.Id == purchaseOrderId);
                    if (poToUpdate != null)
                    {
                        var totalOrdered = poToUpdate.PurchaseOrderItems.Sum(i => i.Quantity);
                        var totalReceived = await db.GoodsReceiptNoteItems
                                                    .Where(gri => gri.GoodsReceiptNote.PurchaseOrderId == purchaseOrderId)
                                                    .SumAsync(gri => gri.QuantityReceived) + itemsToProcess.Sum(i => i.QuantityReceived);

                        if (totalReceived >= totalOrdered)
                        {
                            poToUpdate.ReceiptStatus = ReceiptStatus.FullyReceived;
                            poToUpdate.Status = PurchaseOrderStatus.FullyReceived;
                        }
                        else if (totalReceived > 0)
                        {
                            poToUpdate.ReceiptStatus = ReceiptStatus.PartiallyReceived;
                            poToUpdate.Status = PurchaseOrderStatus.PartiallyReceived;
                        }
                    }
                    // ======================== نهاية الإصلاح الرئيسي ========================


                    // حفظ كل التغييرات مرة واحدة
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw; // إعادة إرسال الخطأ للواجهة
                }
            }
        }


        // ======================= بداية الإضافة =======================
        public async Task<GoodsReceiptNote> GetGoodsReceiptByIdAsync(int grnId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.GoodsReceiptNotes
                    .Include(g => g.PurchaseOrder)
                    .Include(g => g.GoodsReceiptNoteItems)
                    .ThenInclude(gi => gi.Product)
                    .AsNoTracking() // AsNoTracking لتحسين الأداء في وضع القراءة فقط
                    .FirstOrDefaultAsync(g => g.Id == grnId);
            }
        }
        // ======================== نهاية الإضافة ========================

        private async Task UpdateInventoryAfterReceipt(DatabaseContext db, GoodsReceiptItemViewModel itemVM, GoodsReceiptNote grn, int grnItemId)
        {
            // هذا الكود مسؤول عن تحديث المخزون، متوسط التكلفة، إضافة بيانات التتبع، وتسجيل حركة المخزون
            if (itemVM.IsTracked)
            {
                if (itemVM.TrackingMethod == ProductTrackingMethod.BySerialNumber)
                {
                    foreach (var serial in itemVM.SerialNumbers)
                    {
                        db.SerialNumbers.Add(new SerialNumber
                        {
                            ProductId = itemVM.ProductId,
                            Value = serial,
                            StorageLocationId = itemVM.DestinationLocationId.Value,
                            GoodsReceiptNoteItemId = grnItemId,
                            Status = SerialNumberStatus.InStock
                        });
                    }
                }
                else
                {
                    db.LotNumbers.Add(new LotNumber
                    {
                        ProductId = itemVM.ProductId,
                        Value = itemVM.LotInfo.Value,
                        CurrentQuantity = itemVM.QuantityReceived,
                        ExpiryDate = itemVM.LotInfo.ExpiryDate,
                        StorageLocationId = itemVM.DestinationLocationId.Value,
                        GoodsReceiptNoteItemId = grnItemId
                    });
                }
            }

            var product = await db.Products.FindAsync(itemVM.ProductId);
            var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == itemVM.ProductId && i.StorageLocationId == itemVM.DestinationLocationId.Value);

            if (inventory != null)
            {
                var oldTotalValue = inventory.Quantity * product.AverageCost;
                var newTotalValue = itemVM.QuantityReceived * itemVM.UnitPrice;
                var totalQuantity = inventory.Quantity + itemVM.QuantityReceived;
                if (totalQuantity > 0) product.AverageCost = (oldTotalValue + newTotalValue) / totalQuantity;
                inventory.Quantity += itemVM.QuantityReceived;
            }
            else
            {
                db.Inventories.Add(new Inventory { ProductId = itemVM.ProductId, StorageLocationId = itemVM.DestinationLocationId.Value, Quantity = itemVM.QuantityReceived });
                product.AverageCost = itemVM.UnitPrice;
            }

            db.StockMovements.Add(new StockMovement
            {
                ProductId = itemVM.ProductId,
                StorageLocationId = itemVM.DestinationLocationId.Value,
                MovementDate = grn.ReceiptDate,
                MovementType = StockMovementType.PurchaseReceipt,
                Quantity = itemVM.QuantityReceived,
                UnitCost = itemVM.UnitPrice,
                ReferenceDocument = grn.GRNNumber,
                UserId = CurrentUserService.LoggedInUser?.Id
            });
        }
    }
}