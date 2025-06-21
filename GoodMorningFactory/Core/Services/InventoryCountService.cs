// GoodMorningFactory/Core/Services/InventoryCountService.cs
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
    /// خدمة مركزية لإدارة جميع العمليات المتعلقة بأوامر الجرد.
    /// </summary>
    public class InventoryCountService : IInventoryCountService
    {
        public async Task<PaginatedResult<InventoryCount>> GetInventoryCountsAsync(InventoryCountFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.InventoryCounts
                                .Include(ic => ic.Warehouse)
                                .Include(ic => ic.ResponsibleUser)
                                .AsQueryable();

                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string searchTextLower = criteria.SearchText.ToLower();
                    query = query.Where(ic => ic.CountReferenceNumber.ToLower().Contains(searchTextLower) || ic.Warehouse.Name.ToLower().Contains(searchTextLower));
                }

                if (criteria.Status.HasValue)
                {
                    query = query.Where(ic => ic.Status == criteria.Status.Value);
                }

                int totalItems = await query.CountAsync();

                var counts = await query.OrderByDescending(ic => ic.CountDate)
                                     .Skip((criteria.Page - 1) * criteria.PageSize)
                                     .Take(criteria.PageSize)
                                     .ToListAsync();

                return new PaginatedResult<InventoryCount> { Items = counts, TotalCount = totalItems };
            }
        }

        public async Task<AddEditInventoryCountDto> GetInitialDataForCountWindowAsync(int? countId)
        {
            using (var db = new DatabaseContext())
            {
                var dto = new AddEditInventoryCountDto
                {
                    Warehouses = await db.Warehouses.Where(w => w.IsActive).ToListAsync(),
                    Users = await db.Users.Where(u => u.IsActive).ToListAsync()
                };

                if (countId.HasValue)
                {
                    dto.InventoryCount = await db.InventoryCounts
                        .Include(ic => ic.Items).ThenInclude(i => i.Product)
                        .Include(ic => ic.Items).ThenInclude(i => i.StorageLocation)
                        .FirstOrDefaultAsync(ic => ic.Id == countId.Value);

                    if (dto.InventoryCount != null)
                    {
                        foreach (var item in dto.InventoryCount.Items)
                        {
                            dto.Items.Add(new InventoryCountItemViewModel
                            {
                                ProductId = item.ProductId,
                                ProductCode = item.Product.ProductCode,
                                ProductName = item.Product.Name,
                                StorageLocationId = item.StorageLocationId,
                                StorageLocationName = item.StorageLocation.Name,
                                SystemQuantity = item.SystemQuantity,
                                CountedQuantity = item.CountedQuantity
                            });
                        }
                    }
                }
                else
                {
                    dto.InventoryCount = new InventoryCount
                    {
                        CountReferenceNumber = $"IC-{DateTime.Now:yyyyMMddHHmmss}",
                        CountDate = DateTime.Today,
                        Status = InventoryCountStatus.Planned
                    };
                }
                return dto;
            }
        }

        public async Task<List<InventoryCountItemViewModel>> GetProductsForWarehouseAsync(int warehouseId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.StorageLocation)
                    .Where(i => i.StorageLocation.WarehouseId == warehouseId)
                    .Select(inv => new InventoryCountItemViewModel
                    {
                        ProductId = inv.ProductId,
                        ProductCode = inv.Product.ProductCode,
                        ProductName = inv.Product.Name,
                        StorageLocationId = inv.StorageLocationId,
                        StorageLocationName = inv.StorageLocation.Name,
                        SystemQuantity = inv.Quantity,
                        CountedQuantity = inv.Quantity
                    })
                    .ToListAsync();
            }
        }

        public async Task<int> SaveInventoryCountAsync(InventoryCount countFromViewModel, List<InventoryCountItemViewModel> itemsFromViewModel)
        {
            using (var db = new DatabaseContext())
            {
                InventoryCount countInDb;

                if (countFromViewModel.Id == 0) // إنشاء أمر جرد جديد
                {
                    countInDb = new InventoryCount();
                    db.InventoryCounts.Add(countInDb);
                }
                else // تحديث أمر جرد موجود
                {
                    countInDb = await db.InventoryCounts
                                        .Include(ic => ic.Items)
                                        .FirstOrDefaultAsync(ic => ic.Id == countFromViewModel.Id);
                    if (countInDb == null) throw new Exception("لم يتم العثور على أمر الجرد.");
                }

                // تحديث البيانات الأساسية لأمر الجرد
                countInDb.CountReferenceNumber = countFromViewModel.CountReferenceNumber;
                countInDb.CountDate = countFromViewModel.CountDate;
                countInDb.WarehouseId = countFromViewModel.WarehouseId;
                countInDb.ResponsibleUserId = countFromViewModel.ResponsibleUserId;
                countInDb.Notes = countFromViewModel.Notes;
                countInDb.Status = countFromViewModel.Status;

                // حذف البنود القديمة وإضافة الجديدة
                countInDb.Items.Clear();
                foreach (var vm in itemsFromViewModel)
                {
                    countInDb.Items.Add(new InventoryCountItem
                    {
                        ProductId = vm.ProductId,
                        StorageLocationId = vm.StorageLocationId,
                        SystemQuantity = vm.SystemQuantity,
                        CountedQuantity = vm.CountedQuantity
                    });
                }

                await db.SaveChangesAsync();
                return countInDb.Id;
            }
        }

        public async Task PostInventoryCountAsync(int countId)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var count = await db.InventoryCounts.Include(ic => ic.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(ic => ic.Id == countId);
                    if (count == null) throw new Exception("لم يتم العثور على أمر الجرد.");

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultInventoryAccountId == null || companyInfo?.DefaultInventoryAdjustmentAccountId == null)
                    {
                        throw new Exception("يرجى تحديد حساب 'المخزون' و 'تسوية المخزون' الافتراضي في الإعدادات.");
                    }

                    decimal totalAdjustmentValue = 0;

                    foreach (var item in count.Items)
                    {
                        if (item.Difference != 0)
                        {
                            totalAdjustmentValue += item.Difference * item.Product.AverageCost;

                            var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.StorageLocationId == item.StorageLocationId);
                            if (inventory != null)
                            {
                                inventory.Quantity = item.CountedQuantity;
                            }
                            else if (item.CountedQuantity > 0)
                            {
                                db.Inventories.Add(new Inventory { ProductId = item.ProductId, StorageLocationId = item.StorageLocationId, Quantity = item.CountedQuantity });
                            }

                            var movementType = item.Difference > 0 ? StockMovementType.AdjustmentIncrease : StockMovementType.AdjustmentDecrease;
                            db.StockMovements.Add(new StockMovement
                            {
                                ProductId = item.ProductId,
                                StorageLocationId = item.StorageLocationId,
                                // --- بداية التعديل: استخدام الوقت الحالي ---
                                MovementDate = DateTime.Now,
                                // --- نهاية التعديل ---
                                Quantity = Math.Abs(item.Difference),
                                MovementType = movementType,
                                ReferenceDocument = count.CountReferenceNumber,
                                UnitCost = item.Product.AverageCost,
                                UserId = count.ResponsibleUserId
                            });
                        }
                    }

                    if (totalAdjustmentValue != 0)
                    {
                        var voucher = new JournalVoucher
                        {
                            VoucherNumber = $"JV-{count.CountReferenceNumber}",
                            VoucherDate = count.CountDate,
                            Description = $"تسوية فروقات الجرد لأمر الجرد: {count.CountReferenceNumber}",
                            TotalDebit = Math.Abs(totalAdjustmentValue),
                            TotalCredit = Math.Abs(totalAdjustmentValue),
                            Status = VoucherStatus.Posted
                        };

                        if (totalAdjustmentValue > 0)
                        {
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = totalAdjustmentValue, Credit = 0 });
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Debit = 0, Credit = totalAdjustmentValue });
                        }
                        else
                        {
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Debit = Math.Abs(totalAdjustmentValue), Credit = 0 });
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = 0, Credit = Math.Abs(totalAdjustmentValue) });
                        }
                        db.JournalVouchers.Add(voucher);
                    }

                    count.Status = InventoryCountStatus.Completed;
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw; // إعادة رمي الاستثناء ليتم التعامل معه في الـ ViewModel
                }
            }
        }

        public async Task CancelInventoryCountAsync(int countId)
        {
            using (var db = new DatabaseContext())
            {
                var count = await db.InventoryCounts.FindAsync(countId);
                if (count != null)
                {
                    if (count.Status == InventoryCountStatus.Completed || count.Status == InventoryCountStatus.Cancelled)
                    {
                        throw new InvalidOperationException("لا يمكن إلغاء أمر جرد مكتمل أو ملغي بالفعل.");
                    }
                    count.Status = InventoryCountStatus.Cancelled;
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}