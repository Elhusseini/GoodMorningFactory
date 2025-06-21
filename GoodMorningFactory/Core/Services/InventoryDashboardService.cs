// GoodMorningFactory/Core/Services/InventoryDashboardService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية مسؤولة عن حساب وتجميع بيانات لوحة معلومات المخزون.
    /// </summary>
    public class InventoryDashboardService : IInventoryDashboardService
    {
        public async Task<InventoryDashboardDataDto> GetDashboardDataAsync()
        {
            using (var db = new DatabaseContext())
            {
                var dto = new InventoryDashboardDataDto();
                var inventoryItems = await db.Inventories
                                             .Include(i => i.Product)
                                             .ThenInclude(p => p.Category)
                                             .ToListAsync();

                // حساب مؤشرات الأداء الرئيسية
                dto.TotalInventoryValue = inventoryItems.Sum(i => i.Quantity * i.Product.AverageCost);
                dto.LowStockItems = inventoryItems.Count(i => i.Quantity > 0 && i.Quantity <= i.ReorderLevel);
                dto.OutOfStockItems = inventoryItems.Count(i => i.Quantity <= 0);

                // إعداد بيانات الرسم البياني
                var valueByCategory = await db.Categories
                    .Select(cat => new
                    {
                        CategoryName = cat.Name ?? "غير مصنف",
                        TotalValue = db.Inventories
                                       .Where(i => i.Product.CategoryId == cat.Id)
                                       .Sum(i => i.Quantity * i.Product.AverageCost)
                    })
                    .Where(c => c.TotalValue > 0)
                    .ToListAsync();

                dto.ValueByCategorySeries = new SeriesCollection();
                foreach (var category in valueByCategory)
                {
                    dto.ValueByCategorySeries.Add(new PieSeries
                    {
                        Title = category.CategoryName,
                        Values = new ChartValues<decimal> { category.TotalValue },
                        DataLabels = true,
                        LabelPoint = chartPoint => $"{chartPoint.Y:N0} ({chartPoint.Participation:P0})"
                    });
                }

                // استدعاء الدوال الجديدة
                dto.StagnantProducts = await GetTopStagnantProductsAsync(5);
                dto.TopValuedProducts = await GetTopValuedProductsAsync(5);

                return dto;
            }
        }

        public async Task<List<StagnantProductDto>> GetTopStagnantProductsAsync(int count, int stagnantDays = 90)
        {
            using (var db = new DatabaseContext())
            {
                var cutoffDate = DateTime.Now.AddDays(-stagnantDays);

                var lastMovements = await db.StockMovements
                    .GroupBy(m => m.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        LastMovementDate = g.Max(m => m.MovementDate)
                    })
                    .Where(m => m.LastMovementDate < cutoffDate)
                    .ToDictionaryAsync(k => k.ProductId, v => v.LastMovementDate);

                if (!lastMovements.Any())
                {
                    return new List<StagnantProductDto>();
                }

                var productIds = lastMovements.Keys.ToList();

                var stagnantProducts = await db.Inventories
                    .Include(i => i.Product)
                    .Where(i => productIds.Contains(i.ProductId))
                    .GroupBy(i => i.Product)
                    .Select(g => new StagnantProductDto
                    {
                        ProductName = g.Key.Name,
                        QuantityOnHand = g.Sum(i => i.Quantity),
                        DaysSinceLastMovement = (int)(DateTime.Now - lastMovements[g.Key.Id]).TotalDays
                    })
                    .Where(p => p.QuantityOnHand > 0)
                    .OrderByDescending(p => p.DaysSinceLastMovement)
                    .Take(count)
                    .ToListAsync();

                return stagnantProducts;
            }
        }

        // --- بداية التعديل: هنا تم إصلاح المشكلة ---
        public async Task<List<TopValuedProductDto>> GetTopValuedProductsAsync(int count)
        {
            using (var db = new DatabaseContext())
            {
                // الخطوة 1: جلب كل المنتجات وقيمتها من قاعدة البيانات إلى ذاكرة التطبيق
                // تم إزالة OrderByDescending من استعلام قاعدة البيانات
                var allProductsByValue = await db.Inventories
                    .Include(i => i.Product)
                    .GroupBy(i => i.Product)
                    .Select(g => new TopValuedProductDto
                    {
                        ProductName = g.Key.Name,
                        QuantityOnHand = g.Sum(i => i.Quantity),
                        TotalValue = g.Sum(i => i.Quantity * i.Product.AverageCost)
                    })
                    .Where(p => p.QuantityOnHand > 0)
                    .ToListAsync(); // جلب النتائج إلى الذاكرة

                // الخطوة 2: ترتيب النتائج في الذاكرة (client-side) باستخدام LINQ to Objects
                // الآن الترتيب يتم على القائمة الموجودة في الذاكرة، وهذا مدعوم بالكامل
                var topProducts = allProductsByValue
                    .OrderByDescending(p => p.TotalValue)
                    .Take(count)
                    .ToList();

                return topProducts;
            }
        }
        // --- نهاية التعديل ---
    }
}