
// GoodMorningFactory/Core/Services/WorkOrderService.cs

using GoodMorningFactory.Data;

using GoodMorningFactory.Data.Models;

using GoodMorningFactory.UI.ViewModels;

using Microsoft.EntityFrameworkCore;



namespace GoodMorningFactory.Core.Services

{

    public class WorkOrderService : IWorkOrderService

    {

        public async Task<PaginatedResult<WorkOrder>> GetWorkOrdersAsync(WorkOrderFilterCriteria criteria)

        {

            using (var db = new DatabaseContext())

            {

                var query = db.WorkOrders.Include(wo => wo.FinishedGood).AsQueryable();



                if (!string.IsNullOrWhiteSpace(criteria.SearchText))

                {

                    string searchText = criteria.SearchText.ToLower();

                    query = query.Where(wo => wo.WorkOrderNumber.ToLower().Contains(searchText) || wo.FinishedGood.Name.ToLower().Contains(searchText));

                }

                if (criteria.Status.HasValue)

                {

                    query = query.Where(wo => wo.Status == criteria.Status.Value);

                }



                int totalItems = await query.CountAsync();

                var items = await query.OrderByDescending(wo => wo.PlannedStartDate)

                          .Skip((criteria.Page - 1) * criteria.PageSize)

                          .Take(criteria.PageSize)

                          .ToListAsync();



                return new PaginatedResult<WorkOrder> { Items = items, TotalCount = totalItems };

            }

        }

        public async Task<List<object>> GetStatusFiltersAsync()

        {

            await Task.Yield();

            var statuses = new List<object> { "الكل" };

            statuses.AddRange(Enum.GetValues(typeof(WorkOrderStatus)).Cast<object>());

            return statuses;

        }

        public async Task<AddEditWorkOrderDto> GetInitialDataForAddEditWindowAsync(int? workOrderId, int? salesOrderItemId)

        {

            using (var db = new DatabaseContext())

            {

                var dto = new AddEditWorkOrderDto

                {

                    FinishedGoods = await db.Products.Where(p => p.ProductType == ProductType.FinishedGood || p.ProductType == ProductType.WorkInProgress).ToListAsync(),

                    Statuses = Enum.GetValues(typeof(WorkOrderStatus)).Cast<WorkOrderStatus>().ToList()

                };



                if (workOrderId.HasValue)

                {

                    dto.WorkOrder = await db.WorkOrders.FindAsync(workOrderId.Value);

                    var consumedItems = await db.WorkOrderMaterials.Include(m => m.RawMaterial).Where(m => m.WorkOrderId == workOrderId.Value).ToListAsync();

                    dto.ConsumedMaterials = consumedItems.Select(item => new ConsumedMaterialViewModel { MaterialName = item.RawMaterial.Name, QuantityConsumed = item.QuantityConsumed }).ToList();

                }

                else if (salesOrderItemId.HasValue)

                {

                    var salesItem = await db.SalesOrderItems.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == salesOrderItemId.Value);

                    if (salesItem != null)

                    {

                        dto.WorkOrder = new WorkOrder

                        {

                            FinishedGoodId = salesItem.ProductId,

                            QuantityToProduce = salesItem.Quantity,

                            SalesOrderItemId = salesOrderItemId.Value,

                            WorkOrderNumber = $"WO-SO-{DateTime.Now:yyyyMMddHHmmss}",

                            PlannedStartDate = DateTime.Today,

                            PlannedEndDate = DateTime.Today.AddDays(7),

                            Status = WorkOrderStatus.Planned

                        };

                    }

                }

                else

                {

                    dto.WorkOrder = new WorkOrder

                    {

                        WorkOrderNumber = $"WO-{DateTime.Now:yyyyMMddHHmmss}",

                        PlannedStartDate = DateTime.Today,

                        PlannedEndDate = DateTime.Today.AddDays(7),

                        Status = WorkOrderStatus.Planned

                    };

                }



                return dto;

            }

        }

        public async Task<List<RequiredMaterialViewModel>> GetRequiredMaterialsForProductAsync(int productId, int quantity)

        {

            using (var db = new DatabaseContext())

            {

                var bom = await db.BillOfMaterials

                         .Include(b => b.BillOfMaterialsItems)

                         .ThenInclude(i => i.RawMaterial)

                         .FirstOrDefaultAsync(b => b.FinishedGoodId == productId);



                if (bom == null) return new List<RequiredMaterialViewModel>();



                var requiredMaterials = new List<RequiredMaterialViewModel>();

                foreach (var item in bom.BillOfMaterialsItems)

                {

                    var availableStock = await db.Inventories

                      .Where(i => i.ProductId == item.RawMaterialId && i.Quantity > 0)

                      .Include(i => i.StorageLocation)

                      .Select(i => new AvailableStockLocation

                      {

                          StorageLocationId = i.StorageLocationId,

                          LocationName = i.StorageLocation.Name,

                          QuantityOnHand = i.Quantity - i.QuantityReserved

                      })

                      .ToListAsync();



                    requiredMaterials.Add(new RequiredMaterialViewModel

                    {

                        MaterialId = item.RawMaterialId,

                        MaterialName = item.RawMaterial.Name,

                        RequiredQuantity = item.Quantity * quantity,

                        AvailableLocations = availableStock

                    });

                }

                return requiredMaterials;

            }

        }

        public async Task<int> SaveWorkOrderAsync(WorkOrder order, bool isNew)

        {

            using (var db = new DatabaseContext())

            using (var transaction = await db.Database.BeginTransactionAsync())

            {

                try

                {

                    if (isNew)

                    {

                        var bom = await db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).FirstOrDefaultAsync(b => b.FinishedGoodId == order.FinishedGoodId);

                        if (bom == null) throw new Exception("لا توجد قائمة مواد (BOM) لهذا المنتج.");



                        foreach (var item in bom.BillOfMaterialsItems)

                        {

                            var requiredQty = (int)Math.Ceiling(item.Quantity * order.QuantityToProduce);

                            var inventoryItems = await db.Inventories.Where(i => i.ProductId == item.RawMaterialId).ToListAsync();

                            if (inventoryItems.Sum(i => i.Quantity - i.QuantityReserved) < requiredQty)

                            {

                                var material = await db.Products.FindAsync(item.RawMaterialId);

                                throw new Exception($"الكمية غير كافية للمادة: {material?.Name}.");

                            }

                        }



                        foreach (var item in bom.BillOfMaterialsItems)

                        {

                            var requiredQty = (int)Math.Ceiling(item.Quantity * order.QuantityToProduce);

                            var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.RawMaterialId);

                            if (inventory != null)

                            {

                                inventory.QuantityReserved += requiredQty;

                            }

                        }

                    }



                    if (isNew)

                    {

                        db.WorkOrders.Add(order);

                    }

                    else

                    {

                        db.WorkOrders.Update(order);

                    }



                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return order.Id;

                }

                catch (Exception)

                {

                    await transaction.RollbackAsync();

                    throw;

                }

            }

        }

        public async Task UpdateWorkOrderStatusAsync(int workOrderId, WorkOrderStatus newStatus)

        {

            using (var db = new DatabaseContext())

            {

                var order = await db.WorkOrders.FindAsync(workOrderId);

                if (order != null && order.Status != WorkOrderStatus.Completed && order.Status != WorkOrderStatus.Cancelled)

                {

                    order.Status = newStatus;

                    if (newStatus == WorkOrderStatus.InProgress && !order.ActualStartDate.HasValue) order.ActualStartDate = DateTime.Now;

                    if (newStatus == WorkOrderStatus.Completed) order.ActualEndDate = DateTime.Now;



                    if (newStatus == WorkOrderStatus.Cancelled)

                    {

                        await UnreserveMaterialsForCancelledOrderAsync(workOrderId);

                    }

                    await db.SaveChangesAsync();

                }

            }

        }

        private async Task UnreserveMaterialsForCancelledOrderAsync(int workOrderId)

        {

            using (var db = new DatabaseContext())

            {

                var order = await db.WorkOrders.FindAsync(workOrderId);

                if (order == null) return;



                var bom = await db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).FirstOrDefaultAsync(b => b.FinishedGoodId == order.FinishedGoodId);

                var consumed = await db.WorkOrderMaterials.Where(m => m.WorkOrderId == workOrderId).ToListAsync();



                if (bom != null)

                {

                    foreach (var bomItem in bom.BillOfMaterialsItems)

                    {

                        decimal totalPlanned = bomItem.Quantity * order.QuantityToProduce;

                        decimal totalConsumed = consumed.Where(c => c.RawMaterialId == bomItem.RawMaterialId).Sum(c => c.QuantityConsumed);

                        int remainingReservation = (int)Math.Ceiling(totalPlanned - totalConsumed);



                        if (remainingReservation > 0)

                        {

                            var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == bomItem.RawMaterialId);

                            if (inventory != null)

                            {

                                inventory.QuantityReserved -= remainingReservation;

                            }

                        }

                    }

                    await db.SaveChangesAsync();

                }

            }

        }

        public async Task<WorkOrder> GetWorkOrderForConsumptionAsync(int workOrderId) => await new DatabaseContext().WorkOrders.FindAsync(workOrderId);

        public async Task<WorkOrder> GetWorkOrderForProductionReportAsync(int workOrderId) => await new DatabaseContext().WorkOrders.FindAsync(workOrderId);

        public async Task<WorkOrder> GetWorkOrderForLaborRecordAsync(int workOrderId) => await new DatabaseContext().WorkOrders.FindAsync(workOrderId);

        public async Task<MaterialConsumptionDataDto> GetDataForMaterialConsumptionAsync(int workOrderId)

        {

            using (var db = new DatabaseContext())

            {

                var wo = await db.WorkOrders.FindAsync(workOrderId);

                if (wo == null) throw new Exception("لم يتم العثور على أمر العمل.");



                var dto = new MaterialConsumptionDataDto { WorkOrderNumber = wo.WorkOrderNumber };



                var bom = await db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).ThenInclude(i => i.RawMaterial).FirstOrDefaultAsync(b => b.FinishedGoodId == wo.FinishedGoodId);

                if (bom == null) throw new Exception("لم يتم العثور على قائمة مكونات لهذا المنتج.");



                var previouslyConsumed = await db.WorkOrderMaterials

                  .Where(m => m.WorkOrderId == workOrderId)

                  .GroupBy(m => m.RawMaterialId)

                  .ToDictionaryAsync(g => g.Key, g => g.Sum(i => i.QuantityConsumed));



                foreach (var item in bom.BillOfMaterialsItems)

                {

                    var requiredQty = item.Quantity * wo.QuantityToProduce;

                    decimal consumedQty = previouslyConsumed.ContainsKey(item.RawMaterialId) ? previouslyConsumed[item.RawMaterialId] : 0;



                    var availableStock = await db.Inventories

                      .Include(i => i.StorageLocation)

                      .Where(i => i.ProductId == item.RawMaterialId && i.Quantity > 0)

                      .Select(i => new AvailableStockLocation

                      {

                          StorageLocationId = i.StorageLocationId,

                          LocationName = i.StorageLocation.Name,

                          QuantityOnHand = i.Quantity

                      }).ToListAsync();



                    dto.RequiredMaterials.Add(new RequiredMaterialViewModel

                    {

                        MaterialId = item.RawMaterialId,

                        MaterialName = item.RawMaterial.Name,

                        RequiredQuantity = requiredQty,

                        PreviouslyConsumedQuantity = consumedQty,

                        AvailableLocations = availableStock,

                        ConsumedQuantity = 0,

                        SourceLocationId = availableStock.FirstOrDefault()?.StorageLocationId,

                        IsTracked = item.RawMaterial.TrackingMethod != ProductTrackingMethod.None,

                        TrackingMethod = item.RawMaterial.TrackingMethod

                    });

                }

                return dto;

            }

        }

        public async Task ConsumeMaterialsForWorkOrderAsync(int workOrderId, IEnumerable<RequiredMaterialViewModel> itemsToConsume)

        {

            using (var db = new DatabaseContext())

            using (var transaction = await db.Database.BeginTransactionAsync())

            {

                try

                {

                    var workOrder = await db.WorkOrders.FindAsync(workOrderId);

                    if (workOrder == null) throw new Exception("لم يتم العثور على أمر العمل.");



                    foreach (var item in itemsToConsume)

                    {

                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.MaterialId && i.StorageLocationId == item.SourceLocationId.Value);

                        if (inventory == null || inventory.Quantity < item.ConsumedQuantity)

                            throw new Exception($"الكمية غير كافية للمادة: {item.MaterialName}");



                        inventory.Quantity -= (int)item.ConsumedQuantity;

                        inventory.QuantityReserved -= (int)Math.Min(inventory.QuantityReserved, item.ConsumedQuantity);



                        var product = await db.Products.FindAsync(item.MaterialId);

                        db.StockMovements.Add(new StockMovement

                        {

                            ProductId = item.MaterialId,

                            StorageLocationId = item.SourceLocationId.Value,

                            MovementDate = DateTime.Now,

                            MovementType = StockMovementType.ProductionConsumption,

                            Quantity = -(int)item.ConsumedQuantity,

                            UnitCost = product.AverageCost,

                            ReferenceDocument = workOrder.WorkOrderNumber,

                            UserId = CurrentUserService.LoggedInUser?.Id

                        });



                        db.WorkOrderMaterials.Add(new WorkOrderMaterial

                        {

                            WorkOrderId = workOrderId,

                            RawMaterialId = item.MaterialId,

                            QuantityConsumed = item.ConsumedQuantity

                        });



                        if (item.IsTracked && item.TrackingMethod == ProductTrackingMethod.BySerialNumber)

                        {

                            if (item.SelectedSerialIds.Count != (int)item.ConsumedQuantity)

                                throw new Exception($"لم يتم اختيار الأرقام التسلسلية بشكل صحيح للمادة: {item.MaterialName}");



                            var serialsToUpdate = await db.SerialNumbers.Where(sn => item.SelectedSerialIds.Contains(sn.Id)).ToListAsync();

                            foreach (var serial in serialsToUpdate) { serial.Status = SerialNumberStatus.Consumed; }

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

        public async Task<ProductionReportDataDto> GetDataForProductionReportAsync(int workOrderId)

        {

            using (var db = new DatabaseContext())

            {

                var workOrder = await db.WorkOrders.Include(w => w.FinishedGood).FirstOrDefaultAsync(w => w.Id == workOrderId);

                if (workOrder == null) throw new Exception("لم يتم العثور على أمر العمل.");



                return new ProductionReportDataDto

                {

                    WorkOrderNumber = workOrder.WorkOrderNumber,

                    ProductName = workOrder.FinishedGood.Name,

                    OrderedQuantity = workOrder.QuantityToProduce,

                    PreviouslyProduced = workOrder.QuantityProduced,

                    RemainingQuantity = workOrder.QuantityToProduce - workOrder.QuantityProduced

                };

            }

        }

        public async Task ReportProductionAsync(int workOrderId, int producedQuantity, int scrappedQuantity, string scrapReason)

        {

            if (producedQuantity < 0 || scrappedQuantity < 0)

                throw new InvalidOperationException("لا يمكن إدخال كميات سالبة.");



            using (var db = new DatabaseContext())

            using (var transaction = await db.Database.BeginTransactionAsync())

            {

                try

                {

                    var workOrder = await db.WorkOrders.FindAsync(workOrderId);

                    if (workOrder == null) throw new Exception("لم يتم العثور على أمر العمل.");



                    int remainingForOrder = workOrder.QuantityToProduce - workOrder.QuantityProduced;

                    int totalReported = producedQuantity + scrappedQuantity;

                    if (totalReported > remainingForOrder)

                        throw new InvalidOperationException($"إجمالي الكمية المسجلة ({totalReported}) أكبر من الكمية المتبقية ({remainingForOrder}).");



                    if (producedQuantity > 0)

                    {

                        var defaultLocation = await db.StorageLocations.FirstOrDefaultAsync(sl => sl.IsDefault);

                        if (defaultLocation == null) throw new Exception("لا يوجد موقع تخزين افتراضي معرف لاستلام المنتجات.");



                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == workOrder.FinishedGoodId && i.StorageLocationId == defaultLocation.Id);

                        if (inventory != null)

                        {

                            inventory.Quantity += producedQuantity;

                        }

                        else

                        {

                            db.Inventories.Add(new Inventory { ProductId = workOrder.FinishedGoodId, StorageLocationId = defaultLocation.Id, Quantity = producedQuantity });

                        }



                        var product = await db.Products.FindAsync(workOrder.FinishedGoodId);

                        db.StockMovements.Add(new StockMovement

                        {

                            ProductId = workOrder.FinishedGoodId,

                            StorageLocationId = defaultLocation.Id,

                            MovementDate = DateTime.Now,

                            MovementType = StockMovementType.FinishedGoodsProduction,

                            Quantity = producedQuantity,

                            UnitCost = product.AverageCost,

                            ReferenceDocument = workOrder.WorkOrderNumber,

                            UserId = CurrentUserService.LoggedInUser?.Id

                        });

                    }



                    if (scrappedQuantity > 0)

                    {

                        db.WorkOrderScraps.Add(new WorkOrderScrap

                        {

                            WorkOrderId = workOrderId,

                            ProductId = workOrder.FinishedGoodId,

                            Quantity = scrappedQuantity,

                            Reason = scrapReason

                        });

                    }



                    workOrder.QuantityProduced += producedQuantity;

                    workOrder.QuantityScrapped += scrappedQuantity;



                    if ((workOrder.QuantityProduced + workOrder.QuantityScrapped) >= workOrder.QuantityToProduce)

                    {

                        workOrder.Status = WorkOrderStatus.Completed;

                        workOrder.ActualEndDate = DateTime.Now;

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

        public async Task<RecordLaborDataDto> GetDataForLaborRecordAsync(int workOrderId)

        {

            using (var db = new DatabaseContext())

            {

                var workOrder = await db.WorkOrders.FindAsync(workOrderId);

                if (workOrder == null) throw new Exception("لم يتم العثور على أمر العمل.");



                var activeEmployees = await db.Employees.Where(e => e.Status == EmployeeStatus.Active).ToListAsync();



                return new RecordLaborDataDto

                {

                    WorkOrderNumber = workOrder.WorkOrderNumber,

                    ActiveEmployees = activeEmployees

                };

            }

        }

        public async Task RecordLaborAsync(int workOrderId, int employeeId, DateTime workDate, decimal hoursWorked, string description)

        {

            using (var db = new DatabaseContext())

            using (var transaction = await db.Database.BeginTransactionAsync())

            {

                try

                {

                    var workOrder = await db.WorkOrders.FindAsync(workOrderId);

                    var employee = await db.Employees.FindAsync(employeeId);

                    if (workOrder == null || employee == null) throw new Exception("بيانات أمر العمل أو الموظف غير صحيحة.");



                    decimal hourlyRate = (employee.BasicSalary > 0 && 22 * 8 > 0) ? employee.BasicSalary / (22 * 8) : 0;



                    var laborLog = new WorkOrderLaborLog

                    {

                        WorkOrderId = workOrderId,

                        EmployeeId = employeeId,

                        Date = workDate,

                        HoursWorked = hoursWorked,

                        HourlyRate = hourlyRate,

                        Description = description

                    };



                    db.WorkOrderLaborLogs.Add(laborLog);

                    workOrder.TotalLaborCost += laborLog.TotalCost;



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



        // --- بداية الإضافة: تنفيذ دالة إغلاق أمر العمل ---

        public async Task CloseWorkOrderAsync(int workOrderId)

        {

            using (var db = new DatabaseContext())

            using (var transaction = await db.Database.BeginTransactionAsync())

            {

                try

                {

                    var workOrder = await db.WorkOrders

                      .Include(wo => wo.WorkOrderMaterials).ThenInclude(m => m.RawMaterial)

                      .Include(wo => wo.WorkOrderLaborLogs)

                      .FirstOrDefaultAsync(wo => wo.Id == workOrderId);



                    if (workOrder == null) throw new Exception("لم يتم العثور على أمر العمل.");

                    if (workOrder.Status != WorkOrderStatus.Completed) throw new InvalidOperationException("لا يمكن إغلاق أمر العمل إلا إذا كانت حالته 'مكتمل'.");



                    decimal totalMaterialCost = 0;

                    foreach (var material in workOrder.WorkOrderMaterials)

                    {

                        totalMaterialCost += material.QuantityConsumed * material.RawMaterial.AverageCost;

                    }



                    decimal totalLaborCost = workOrder.TotalLaborCost;

                    decimal totalProductionCost = totalMaterialCost + totalLaborCost;

                    if (workOrder.QuantityProduced == 0) throw new InvalidOperationException("لا يمكن إغلاق أمر عمل بكمية منتجة صفرية.");



                    decimal unitCost = totalProductionCost / workOrder.QuantityProduced;

                    var finishedGood = await db.Products.FindAsync(workOrder.FinishedGoodId);



                    var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == finishedGood.Id);

                    int oldQuantity = inventory?.Quantity - workOrder.QuantityProduced ?? 0;



                    finishedGood.AverageCost = ((oldQuantity * finishedGood.AverageCost) + (workOrder.QuantityProduced * unitCost)) / (oldQuantity + workOrder.QuantityProduced);



                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();

                    if (companyInfo?.DefaultWipAccountId == null || companyInfo?.DefaultInventoryAccountId == null)

                        throw new Exception("يرجى تحديد حسابات الإنتاج تحت التشغيل والمخزون الافتراضية في الإعدادات.");



                    var jv = new JournalVoucher

                    {

                        VoucherNumber = $"JV-WOC-{workOrder.WorkOrderNumber}",

                        VoucherDate = DateTime.Today,

                        Description = $"إغلاق أمر العمل رقم {workOrder.WorkOrderNumber} وتكلفة إنتاج {workOrder.QuantityProduced} وحدة من المنتج '{finishedGood.Name}'",

                        TotalDebit = totalProductionCost,

                        TotalCredit = totalProductionCost,

                        Status = VoucherStatus.Posted

                    };

                    jv.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = totalProductionCost, Credit = 0 });

                    jv.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultWipAccountId.Value, Debit = 0, Credit = totalProductionCost });



                    db.JournalVouchers.Add(jv);



                    workOrder.ActualEndDate = workOrder.ActualEndDate ?? DateTime.Now;



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

        // --- نهاية الإضافة ---

    }

}