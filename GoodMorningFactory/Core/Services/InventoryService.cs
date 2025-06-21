// GoodMorningFactory/Core/Services/InventoryService.cs
// *** الكود الكامل والمعدل - تمت إضافة تنفيذ الدوال الجديدة ***
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
    public class InventoryService : IInventoryService
    {
        // --- الدوال الموجودة مسبقاً (تبقى كما هي دون اختصار) ---
        public async Task<PaginatedResult<GroupedInventoryViewModel>> GetInventoryStatusAsync(InventoryFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Inventories
                  .Include(i => i.Product).ThenInclude(p => p.Category)
                  .Include(i => i.StorageLocation).ThenInclude(sl => sl.Warehouse)
                  .AsQueryable();

                // تطبيق الفلاتر الأولية على مستوى المنتج
                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string searchTextLower = criteria.SearchText.ToLower();
                    query = query.Where(i => i.Product.Name.ToLower().Contains(searchTextLower) ||
                               i.Product.ProductCode.ToLower().Contains(searchTextLower));
                }

                if (criteria.WarehouseId.HasValue && criteria.WarehouseId > 0)
                {
                    query = query.Where(i => i.StorageLocation.WarehouseId == criteria.WarehouseId.Value);
                }

                if (criteria.CategoryId.HasValue && criteria.CategoryId > 0)
                {
                    query = query.Where(i => i.Product.CategoryId == criteria.CategoryId.Value);
                }

                // تجميع الأرصدة حسب المنتج
                var groupedQuery = query
                    .GroupBy(i => i.Product)
                    .Select(g => new GroupedInventoryViewModel
                    {
                        ProductId = g.Key.Id,
                        ProductCode = g.Key.ProductCode,
                        ProductName = g.Key.Name,
                        CategoryName = g.Key.Category.Name ?? "غير مصنف",
                        AverageCost = g.Key.AverageCost,
                        LastPurchasePrice = g.Key.PurchasePrice,
                        LocationDetails = g.Select(inv => new InventoryLocationDetailViewModel
                        {
                            ProductId = inv.ProductId,
                            StorageLocationId = inv.StorageLocationId,
                            WarehouseName = inv.StorageLocation.Warehouse.Name,
                            StorageLocationName = inv.StorageLocation.Name,
                            QuantityOnHand = inv.Quantity,
                            QuantityReserved = inv.QuantityReserved,
                            ReorderLevel = inv.ReorderLevel,
                            MinStockLevel = inv.MinStockLevel,
                            MaxStockLevel = inv.MaxStockLevel,
                        }).ToList()
                    });

                // جلب كل النتائج المجمّعة إلى الذاكرة لتطبيق فلتر الحالة
                var allItems = await groupedQuery.ToListAsync();

                // تطبيق فلتر الحالة (Status) في الذاكرة بعد حساب القيم المجمّعة
                if (criteria.Status.HasValue)
                {
                    allItems = allItems.Where(i => i.Status == criteria.Status.Value).ToList();
                }

                int totalItems = allItems.Count;

                // تطبيق الترتيب والتقسيم للصفحات
                var pagedItems = allItems
                  .OrderBy(i => i.ProductName)
                  .Skip((criteria.Page - 1) * criteria.PageSize)
                  .Take(criteria.PageSize)
                  .ToList();

                return new PaginatedResult<GroupedInventoryViewModel> { Items = pagedItems, TotalCount = totalItems };
            }
        }
        // --- نهاية التعديل ---

        public async Task<InventoryFilters> GetInventoryFiltersAsync()
        {
            using (var db = new DatabaseContext())
            {
                var warehouses = await db.Warehouses.Where(w => w.IsActive).ToListAsync();
                var categories = await db.Categories.ToListAsync();

                var statuses = new List<object> { "الكل" };
                statuses.AddRange(Enum.GetValues(typeof(StockStatus)).Cast<object>());

                return new InventoryFilters
                {
                    Warehouses = warehouses,
                    Categories = categories,
                    Statuses = statuses
                };
            }
        }

        public async Task<QuickAdjustDataDto> GetDataForQuickAdjustmentAsync(int productId, int storageLocationId)
        {
            using (var db = new DatabaseContext())
            {
                var inventory = await db.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.StorageLocation)
                    .FirstOrDefaultAsync(i => i.ProductId == productId && i.StorageLocationId == storageLocationId);

                if (inventory == null)
                {
                    var product = await db.Products.FindAsync(productId);
                    var location = await db.StorageLocations.FindAsync(storageLocationId);
                    if (product == null || location == null) throw new Exception("المنتج أو الموقع غير موجود.");
                    return new QuickAdjustDataDto
                    {
                        ProductName = product.Name,
                        StorageLocationName = location.Name,
                        SystemQuantity = 0
                    };
                }

                return new QuickAdjustDataDto
                {
                    ProductName = inventory.Product.Name,
                    StorageLocationName = inventory.StorageLocation.Name,
                    SystemQuantity = inventory.Quantity
                };
            }
        }

        public async Task PerformQuickAdjustmentAsync(int productId, int storageLocationId, int newQuantity, string reason)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var inventory = await db.Inventories.Include(i => i.Product).FirstOrDefaultAsync(i => i.ProductId == productId && i.StorageLocationId == storageLocationId);

                    int oldQuantity = inventory?.Quantity ?? 0;
                    int difference = newQuantity - oldQuantity;

                    if (difference == 0) return;

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultInventoryAccountId == null || companyInfo?.DefaultInventoryAdjustmentAccountId == null)
                        throw new Exception("يرجى تحديد حساب 'المخزون' و 'تسوية المخزون' في الإعدادات.");

                    if (inventory != null)
                    {
                        inventory.Quantity = newQuantity;
                        inventory.LastUpdated = DateTime.Now;
                    }
                    else
                    {
                        inventory = new Inventory { ProductId = productId, StorageLocationId = storageLocationId, Quantity = newQuantity, LastUpdated = DateTime.Now };
                        db.Inventories.Add(inventory);
                    }

                    await db.SaveChangesAsync();

                    var movementType = difference > 0 ? StockMovementType.AdjustmentIncrease : StockMovementType.AdjustmentDecrease;
                    var reference = $"ADJ-QCK-{DateTime.Now:yyyyMMddHHmmss}";
                    var product = inventory.Product ?? await db.Products.FindAsync(productId);
                    decimal adjustmentValue = Math.Abs(difference) * product.AverageCost;

                    db.StockMovements.Add(new StockMovement { ProductId = productId, StorageLocationId = storageLocationId, MovementDate = DateTime.Now, MovementType = movementType, Quantity = Math.Abs(difference), UnitCost = product.AverageCost, ReferenceDocument = reference, UserId = CurrentUserService.LoggedInUser?.Id });

                    if (adjustmentValue > 0)
                    {
                        var voucher = new JournalVoucher { VoucherNumber = $"JV-{reference}", VoucherDate = DateTime.Today, Description = $"تسوية مخزون للمنتج: {product.Name}. السبب: {reason}", TotalDebit = adjustmentValue, TotalCredit = adjustmentValue, Status = VoucherStatus.Posted };
                        if (difference > 0) { voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = adjustmentValue }); voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Credit = adjustmentValue }); }
                        else { voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Debit = adjustmentValue }); voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Credit = adjustmentValue }); }
                        db.JournalVouchers.Add(voucher);
                    }

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

        // --- بداية الإضافة: تنفيذ الدوال الجديدة ---
        public async Task<List<ProductStockLocationViewModel>> GetStockLevelsForProductAsync(int productId)
        {
            using (var db = new DatabaseContext())
            {
                var allLocations = await db.StorageLocations.Where(sl => sl.IsActive).OrderBy(sl => sl.Name).ToListAsync();

                var currentInventories = await db.Inventories
                    .Where(i => i.ProductId == productId)
                    .ToDictionaryAsync(i => i.StorageLocationId, i => i.Quantity);

                var result = allLocations.Select(location => new ProductStockLocationViewModel
                {
                    StorageLocationId = location.Id,
                    StorageLocationName = location.Name,
                    CurrentQuantity = currentInventories.ContainsKey(location.Id) ? currentInventories[location.Id] : 0,
                    NewQuantity = currentInventories.ContainsKey(location.Id) ? currentInventories[location.Id] : 0
                }).ToList();

                return result;
            }
        }

        public async Task UpdateStockLevelsForProductAsync(int productId, IEnumerable<ProductStockLocationViewModel> stockLevels, string reason)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var product = await db.Products.FindAsync(productId);
                    if (product == null) throw new Exception("لم يتم العثور على المنتج.");

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultInventoryAccountId == null || companyInfo?.DefaultInventoryAdjustmentAccountId == null)
                        throw new Exception("يرجى تحديد حساب 'المخزون' و 'تسوية المخزون' في الإعدادات.");

                    foreach (var level in stockLevels)
                    {
                        if (level.NewQuantity == level.CurrentQuantity) continue;

                        int difference = level.NewQuantity - level.CurrentQuantity;

                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId && i.StorageLocationId == level.StorageLocationId);

                        if (inventory != null)
                        {
                            inventory.Quantity = level.NewQuantity;
                            inventory.LastUpdated = DateTime.Now;
                        }
                        else
                        {
                            inventory = new Inventory { ProductId = productId, StorageLocationId = level.StorageLocationId, Quantity = level.NewQuantity, LastUpdated = DateTime.Now };
                            db.Inventories.Add(inventory);
                        }

                        var movementType = difference > 0 ? StockMovementType.AdjustmentIncrease : StockMovementType.AdjustmentDecrease;
                        var reference = $"ADJ-MNG-{DateTime.Now:yyyyMMddHHmmss}";
                        decimal adjustmentValue = Math.Abs(difference) * product.AverageCost;

                        db.StockMovements.Add(new StockMovement { ProductId = productId, StorageLocationId = level.StorageLocationId, MovementDate = DateTime.Now, MovementType = movementType, Quantity = Math.Abs(difference), UnitCost = product.AverageCost, ReferenceDocument = reference, UserId = CurrentUserService.LoggedInUser?.Id });

                        if (adjustmentValue > 0)
                        {
                            var voucher = new JournalVoucher { VoucherNumber = $"JV-{reference}-{level.StorageLocationId}", VoucherDate = DateTime.Today, Description = $"تسوية مخزون للمنتج '{product.Name}' في الموقع '{level.StorageLocationName}'. السبب: {reason}", TotalDebit = adjustmentValue, TotalCredit = adjustmentValue, Status = VoucherStatus.Posted };
                            if (difference > 0) { voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = adjustmentValue }); voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Credit = adjustmentValue }); }
                            else { voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Debit = adjustmentValue }); voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Credit = adjustmentValue }); }
                            db.JournalVouchers.Add(voucher);
                        }
                    }

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
        // --- بداية تنفيذ الدوال الجديدة ---
        public async Task<List<Warehouse>> GetActiveWarehousesAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Warehouses.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync();
            }
        }

        public async Task<List<StorageLocation>> GetActiveLocationsByWarehouseAsync(int warehouseId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.StorageLocations.Where(l => l.WarehouseId == warehouseId && l.IsActive).OrderBy(l => l.Name).ToListAsync();
            }
        }

        public async Task<StockAdjustmentItemViewModel> GetProductForAdjustmentAsync(string searchText, int locationId)
        {
            using (var db = new DatabaseContext())
            {
                var product = await db.Products.FirstOrDefaultAsync(p => p.ProductCode.ToLower() == searchText.ToLower() || p.Name.ToLower().Contains(searchText.ToLower()));
                if (product == null) return null;

                var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == product.Id && i.StorageLocationId == locationId);
                int systemQty = inventory?.Quantity ?? 0;

                return new StockAdjustmentItemViewModel
                {
                    ProductId = product.Id,
                    ProductCode = product.ProductCode,
                    ProductName = product.Name,
                    SystemQuantity = systemQty,
                    ActualQuantity = systemQty,
                    UnitCost = product.AverageCost
                };
            }
        }

        // ======================= بداية الإصلاح الرئيسي =======================
        public async Task PostStockAdjustmentAsync(StockAdjustment adjustment)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultInventoryAccountId == null || companyInfo?.DefaultInventoryAdjustmentAccountId == null)
                        throw new Exception("يرجى تحديد حساب 'المخزون' و 'تسوية المخزون' في الإعدادات.");

                    // الخطوة 1: حفظ رأس التسوية أولاً
                    db.StockAdjustments.Add(adjustment);

                    var allProductNames = new List<string>();
                    var voucherItems = new List<JournalVoucherItem>();
                    decimal totalAdjustmentValueIncrease = 0;
                    decimal totalAdjustmentValueDecrease = 0;

                    foreach (var item in adjustment.StockAdjustmentItems)
                    {
                        // جلب المنتج لتجنب تكرار الاستعلامات
                        var product = await db.Products.FindAsync(item.ProductId);
                        if (product == null) continue; // تخطي المنتج إذا لم يتم العثور عليه
                        allProductNames.Add(product.Name);

                        // الخطوة 2: تحديث كمية المخزون
                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.StorageLocationId == adjustment.StorageLocationId.Value);
                        if (inventory != null)
                        {
                            inventory.Quantity = item.CountedQuantity;
                        }
                        else
                        {
                            db.Inventories.Add(new Inventory { ProductId = item.ProductId, StorageLocationId = adjustment.StorageLocationId.Value, Quantity = item.CountedQuantity, LastUpdated = DateTime.Now });
                        }

                        // الخطوة 3: إنشاء حركة مخزنية
                        int difference = item.CountedQuantity - item.SystemQuantity;
                        var movementType = difference > 0 ? StockMovementType.AdjustmentIncrease : StockMovementType.AdjustmentDecrease;
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = item.ProductId,
                            StorageLocationId = adjustment.StorageLocationId.Value,
                            MovementDate = adjustment.AdjustmentDate,
                            MovementType = movementType,
                            Quantity = Math.Abs(difference),
                            UnitCost = product.AverageCost, // استخدام تكلفة المنتج
                            ReferenceDocument = adjustment.ReferenceNumber,
                            UserId = CurrentUserService.LoggedInUser?.Id
                        });

                        // الخطوة 4: تجميع قيمة القيد اليومي
                        decimal itemAdjustmentValue = difference * product.AverageCost;
                        if (itemAdjustmentValue > 0) totalAdjustmentValueIncrease += itemAdjustmentValue;
                        else totalAdjustmentValueDecrease += Math.Abs(itemAdjustmentValue);
                    }

                    // الخطوة 5: إنشاء قيد يومية مجمع
                    if (totalAdjustmentValueIncrease > 0 || totalAdjustmentValueDecrease > 0)
                    {
                        var voucher = new JournalVoucher
                        {
                            VoucherNumber = $"JV-{adjustment.ReferenceNumber}",
                            VoucherDate = adjustment.AdjustmentDate,
                            Description = $"تسوية مخزون لـ: {string.Join(", ", allProductNames)}",
                            Status = VoucherStatus.Posted
                        };

                        if (totalAdjustmentValueIncrease > 0)
                        {
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = totalAdjustmentValueIncrease, Credit = 0, Description = "زيادة المخزون نتيجة الجرد" });
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Debit = 0, Credit = totalAdjustmentValueIncrease, Description = "زيادة المخزون نتيجة الجرد" });
                        }
                        if (totalAdjustmentValueDecrease > 0)
                        {
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Debit = totalAdjustmentValueDecrease, Credit = 0, Description = "نقص المخزون نتيجة الجرد" });
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = 0, Credit = totalAdjustmentValueDecrease, Description = "نقص المخزون نتيجة الجرد" });
                        }

                        voucher.TotalDebit = voucher.JournalVoucherItems.Sum(i => i.Debit);
                        voucher.TotalCredit = voucher.JournalVoucherItems.Sum(i => i.Credit);
                        db.JournalVouchers.Add(voucher);
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"حدث خطأ أثناء الحفظ. التفاصيل: {ex.Message}", ex);
                }
            }
        }
        // ======================== نهاية الإصلاح الرئيسي ========================

        // ======================= بداية الإضافة =======================
        public async Task<List<LowStockNotificationViewModel>> GetLowStockItemsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Inventories
                    .Include(i => i.Product).ThenInclude(p => p.DefaultSupplier)
                    .Include(i => i.StorageLocation).ThenInclude(sl => sl.Warehouse)
                    .Where(i => i.Quantity <= i.ReorderLevel && i.ReorderLevel > 0)
                    .Select(i => new LowStockNotificationViewModel
                    {
                        ProductId = i.ProductId,
                        ProductCode = i.Product.ProductCode,
                        ProductName = i.Product.Name,
                        WarehouseName = i.StorageLocation.Warehouse.Name,
                        QuantityOnHand = i.Quantity,
                        ReorderLevel = i.ReorderLevel,
                        DefaultSupplierName = i.Product.DefaultSupplier != null ? i.Product.DefaultSupplier.Name : "غير محدد"
                    })
                    .ToListAsync();
            }
        }
        // ======================== نهاية الإضافة ========================


        // ======================= بداية الإضافة =======================
        public async Task<List<StockMovementViewModel>> GetStockMovementsForProductAsync(int productId)
        {
            using (var db = new DatabaseContext())
            {
                var movements = await db.StockMovements
                    .Where(m => m.ProductId == productId)
                    .Include(m => m.StorageLocation.Warehouse)
                    .Include(m => m.User)
                    .OrderByDescending(m => m.MovementDate)
                    .ToListAsync();

                var viewModels = movements.Select(m => new StockMovementViewModel
                {
                    Date = m.MovementDate,
                    MovementType = m.MovementType,
                    ReferenceNumber = m.ReferenceDocument,
                    ProductName = m.Product?.Name, // Product is already loaded if this method is called from a product context
                    WarehouseName = m.StorageLocation?.Warehouse?.Name,
                    StorageLocationName = m.StorageLocation?.Name,
                    QuantityIn = (m.MovementType == StockMovementType.PurchaseReceipt || m.MovementType == StockMovementType.FinishedGoodsProduction || m.MovementType == StockMovementType.AdjustmentIncrease || m.MovementType == StockMovementType.TransferIn || m.MovementType == StockMovementType.SalesReturn) ? m.Quantity : 0,
                    QuantityOut = (m.MovementType == StockMovementType.SalesShipment || m.MovementType == StockMovementType.ProductionConsumption || m.MovementType == StockMovementType.AdjustmentDecrease || m.MovementType == StockMovementType.TransferOut || m.MovementType == StockMovementType.PurchaseReturn) ? m.Quantity : 0,
                    UserName = m.User?.Username ?? "System"
                }).ToList();

                return viewModels;
            }
        }
        // ======================== نهاية الإضافة ========================


        // ======================= بداية الإضافة =======================
        public async Task<List<SerialNumber>> GetSerialNumbersAsync(string filter)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.SerialNumbers
                    .Include(sn => sn.Product)
                    .Include(sn => sn.StorageLocation.Warehouse)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    query = query.Where(sn => sn.Value.Contains(filter));
                }

                return await query.OrderByDescending(sn => sn.Id).Take(200).ToListAsync();
            }
        }
        // ======================== نهاية الإضافة ========================
    }
}
