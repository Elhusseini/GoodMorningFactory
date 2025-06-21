// Core/Services/MRPService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة تخطيط متطلبات المواد (MRP) ***

using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// الفئة التي تحتوي على المنطق الفعلي لحساب تخطيط متطلبات المواد.
    /// </summary>
    public class MRPService : IMRPService
    {
        public async Task<List<MRPResultViewModel>> RunMRPAsync()
        {
            using (var db = new DatabaseContext())
            {
                // الخطوة 1: جلب كل أوامر البيع المفتوحة التي تحتاج إلى تصنيع منتجات.
                var openSalesOrderItems = await db.SalesOrderItems
                    .Include(soi => soi.Product)
                    .Where(soi => soi.SalesOrder.Status != OrderStatus.Shipped &&
                                  soi.SalesOrder.Status != OrderStatus.Invoiced &&
                                  soi.SalesOrder.Status != OrderStatus.Cancelled)
                    .Where(soi => soi.Product.ProductType == ProductType.FinishedGood ||
                                  soi.Product.ProductType == ProductType.WorkInProgress)
                    .ToListAsync();

                // الخطوة 2: حساب إجمالي الاحتياجات (Gross Requirements) من المواد الخام.
                var grossRequirements = new Dictionary<int, decimal>();
                var boms = await db.BillOfMaterials
                                   .Include(b => b.BillOfMaterialsItems)
                                   .AsNoTracking()
                                   .ToListAsync();

                foreach (var item in openSalesOrderItems)
                {
                    var bom = boms.FirstOrDefault(b => b.FinishedGoodId == item.ProductId);
                    if (bom != null)
                    {
                        foreach (var material in bom.BillOfMaterialsItems)
                        {
                            decimal requiredQty = material.Quantity * item.Quantity;
                            if (grossRequirements.ContainsKey(material.RawMaterialId))
                            {
                                grossRequirements[material.RawMaterialId] += requiredQty;
                            }
                            else
                            {
                                grossRequirements[material.RawMaterialId] = requiredQty;
                            }
                        }
                    }
                }

                // الخطوة 3: جلب بيانات المخزون الحالي والكميات المجدولة للاستلام.
                var onHandInventory = await db.Inventories.AsNoTracking().ToDictionaryAsync(i => i.ProductId, i => i.Quantity);
                var scheduledReceipts = await db.PurchaseOrderItems
                    .Where(poi => poi.PurchaseOrder.Status != PurchaseOrderStatus.FullyReceived &&
                                  poi.PurchaseOrder.Status != PurchaseOrderStatus.Cancelled)
                    .GroupBy(poi => poi.ProductId)
                    .ToDictionaryAsync(g => g.Key, g => g.Sum(item => item.Quantity));

                // الخطوة 4: تجميع النتائج وحساب صافي الاحتياج (Net Requirements).
                var mrpResults = new List<MRPResultViewModel>();
                var allRequiredMaterials = await db.Products
                    .Where(p => grossRequirements.Keys.Contains(p.Id))
                    .Include(p => p.UnitOfMeasure)
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var material in allRequiredMaterials)
                {
                    decimal grossReq = grossRequirements.GetValueOrDefault(material.Id, 0);
                    int onHand = onHandInventory.GetValueOrDefault(material.Id, 0);
                    int scheduled = scheduledReceipts.GetValueOrDefault(material.Id, 0);
                    decimal netReq = grossReq - (onHand + scheduled);

                    // نضيف فقط المواد التي نحتاج إلى شرائها
                    if (netReq > 0)
                    {
                        mrpResults.Add(new MRPResultViewModel
                        {
                            ProductId = material.Id,
                            ProductCode = material.ProductCode,
                            ProductName = material.Name,
                            UnitOfMeasure = material.UnitOfMeasure?.Name ?? "N/A",
                            GrossRequirements = grossReq,
                            OnHandInventory = onHand,
                            ScheduledReceipts = scheduled,
                            NetRequirements = netReq
                        });
                    }
                }

                return mrpResults.OrderBy(r => r.ProductName).ToList();
            }
        }
    }
}