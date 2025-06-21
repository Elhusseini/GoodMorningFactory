// GoodMorningFactory/Core/Services/ProductHistoryService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels; // <-- إضافة مهمة
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class ProductHistoryService : IProductHistoryService
    {
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Products.FindAsync(productId);
            }
        }

        // --- بداية التعديل: إعادة كتابة الدالة بالكامل لتجميع البيانات ---
        public async Task<List<PurchaseHistoryViewModel>> GetPurchaseHistoryForProductAsync(int productId)
        {
            using (var db = new DatabaseContext())
            {
                // جلب كل بنود الشراء للمنتج المحدد مع تضمين بيانات الفاتورة والمورد
                var purchaseItems = await db.PurchaseItems
                               .Include(pi => pi.Purchase)
                               .ThenInclude(p => p.Supplier)
                               .Where(pi => pi.ProductId == productId)
                               .ToListAsync(); // جلب البيانات للذاكرة لتسهيل عملية التجميع

                // تجميع البنود في الذاكرة بناءً على الفاتورة وسعر الوحدة
                var groupedHistory = purchaseItems
                    .GroupBy(pi => new { pi.Purchase, pi.UnitPrice })
                    .Select(g => new PurchaseHistoryViewModel
                    {
                        PurchaseDate = g.Key.Purchase.PurchaseDate,
                        InvoiceNumber = g.Key.Purchase.InvoiceNumber,
                        SupplierName = g.Key.Purchase.Supplier.Name,
                        Quantity = g.Sum(item => item.Quantity), // حساب إجمالي الكمية للمجموعة
                        UnitPrice = g.Key.UnitPrice // سعر الوحدة هو نفسه للمجموعة
                    })
                    .OrderByDescending(result => result.PurchaseDate)
                    .ToList();

                return groupedHistory;
            }
        }
        // --- نهاية التعديل ---
    }
}