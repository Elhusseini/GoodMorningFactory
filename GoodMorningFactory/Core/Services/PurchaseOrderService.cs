// GoodMorningFactory/Core/Services/PurchaseOrderService.cs
// *** الكود الكامل والنهائي ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        public async Task<PagedResult<PurchaseOrder>> GetPurchaseOrdersAsync(int page, int pageSize, string searchText, PurchaseOrderStatus? status)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.PurchaseOrders.Include(po => po.Supplier).AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(po => po.PurchaseOrderNumber.ToLower().Contains(searchText.ToLower()) || po.Supplier.Name.ToLower().Contains(searchText.ToLower()));
                }
                if (status.HasValue)
                {
                    query = query.Where(po => po.Status == status.Value);
                }

                var totalItems = await query.CountAsync();
                var items = await query.OrderByDescending(po => po.OrderDate)
                                       .Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();

                return new PagedResult<PurchaseOrder> { Items = items, TotalItems = totalItems };
            }
        }

        public async Task<PurchaseOrderDto> GetDataForAddEditAsync(int? poId)
        {
            using (var db = new DatabaseContext())
            {
                var dto = new PurchaseOrderDto
                {
                    AllSuppliers = await db.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(),
                    AllProducts = await db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync()
                };

                if (poId.HasValue)
                {
                    dto.Order = await db.PurchaseOrders
                                        .Include(p => p.PurchaseOrderItems)
                                        .FirstOrDefaultAsync(p => p.Id == poId.Value);
                }
                else
                {
                    dto.Order = new PurchaseOrder(); // أمر جديد فارغ
                }
                return dto;
            }
        }

        public async Task SavePurchaseOrderAsync(PurchaseOrder order)
        {
            using (var db = new DatabaseContext())
            {
                if (order.Id > 0) // تحديث أمر حالي
                {
                    var existingOrder = await db.PurchaseOrders.Include(p => p.PurchaseOrderItems).FirstOrDefaultAsync(p => p.Id == order.Id);
                    if (existingOrder != null)
                    {
                        // تحديث الخصائص الأساسية
                        db.Entry(existingOrder).CurrentValues.SetValues(order);

                        // حذف البنود القديمة
                        db.PurchaseOrderItems.RemoveRange(existingOrder.PurchaseOrderItems);

                        // إضافة البنود الجديدة
                        foreach (var item in order.PurchaseOrderItems)
                        {
                            existingOrder.PurchaseOrderItems.Add(new PurchaseOrderItem
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice
                            });
                        }
                    }
                }
                else // إنشاء أمر جديد
                {
                    order.PurchaseOrderNumber = $"PO-{DateTime.Now:yyyyMMddHHmmss}";
                    db.PurchaseOrders.Add(order);

                    // ======================= بداية الإصلاح الرئيسي =======================
                    if (order.PurchaseRequisitionId.HasValue)
                    {
                        var sourceRequisition = await db.PurchaseRequisitions.FindAsync(order.PurchaseRequisitionId.Value);
                        if (sourceRequisition != null)
                        {
                            sourceRequisition.Status = RequisitionStatus.PO_Created;
                        }
                    }
                    // ======================== نهاية الإصلاح الرئيسي ========================
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task CancelPurchaseOrderAsync(int poId)
        {
            using (var db = new DatabaseContext())
            {
                var order = await db.PurchaseOrders.FindAsync(poId);
                if (order != null)
                {
                    order.Status = PurchaseOrderStatus.Cancelled;
                    await db.SaveChangesAsync();
                }
            }
        }
        // ======================= بداية الإضافة =======================
        public async Task<PurchaseOrderDto> GetDataForPOFromRequisitionAsync(int requisitionId)
        {
            using (var db = new DatabaseContext())
            {
                // 1. جلب البيانات الأساسية (الموردين والمنتجات)
                var dto = new PurchaseOrderDto
                {
                    AllSuppliers = await db.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(),
                    AllProducts = await db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync()
                };

                // 2. جلب طلب الشراء المصدر مع بنوده
                var requisition = await db.PurchaseRequisitions
                    .Include(pr => pr.PurchaseRequisitionItems)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(pr => pr.Id == requisitionId);

                if (requisition == null)
                {
                    // إذا لم يتم العثور على الطلب، نرجع ببيانات فارغة
                    dto.Order = new PurchaseOrder();
                    return dto;
                }

                // 3. إنشاء أمر شراء جديد وتعبئته من طلب الشراء
                var newOrder = new PurchaseOrder
                {
                    PurchaseRequisitionId = requisition.Id, // ربط أمر الشراء بالطلب
                    OrderDate = DateTime.Today,
                    ExpectedDeliveryDate = DateTime.Today.AddDays(7), // تاريخ افتراضي
                    Status = PurchaseOrderStatus.Draft
                };

                // 4. تحويل بنود طلب الشراء إلى بنود أمر شراء
                foreach (var reqItem in requisition.PurchaseRequisitionItems)
                {
                    var product = dto.AllProducts.FirstOrDefault(p => p.Id == reqItem.ProductId);
                    newOrder.PurchaseOrderItems.Add(new PurchaseOrderItem
                    {
                        ProductId = reqItem.ProductId ?? 0,
                        Quantity = (int)reqItem.Quantity,
                        UnitPrice = product?.PurchasePrice ?? 0 // استخدام سعر الشراء الافتراضي للمنتج
                    });
                }

                dto.Order = newOrder;
                return dto;
            }
        }
        // ======================== نهاية الإضافة ========================
    }
}