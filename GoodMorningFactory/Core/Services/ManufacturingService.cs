// Services/ManufacturingService.cs
// التنفيذ الفعلي لواجهة IManufacturingService
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Services
{
    public class ManufacturingService : IManufacturingService
    {
        private DatabaseContext CreateDbContext() => new DatabaseContext();

        public async Task<List<Product>> GetFinishedGoodsAsync()
        {
            using (var db = CreateDbContext())
            {
                return await db.Products.Where(p => p.ProductType == ProductType.FinishedGood).ToListAsync();
            }
        }

        public async Task<List<Product>> GetRawMaterialsAsync()
        {
            using (var db = CreateDbContext())
            {
                return await db.Products.Where(p => p.ProductType == ProductType.RawMaterial).ToListAsync();
            }
        }

        public async Task<Product> FindRawMaterialAsync(string searchTerm)
        {
            using (var db = CreateDbContext())
            {
                var lowerSearchTerm = searchTerm.ToLower();
                return await db.Products.FirstOrDefaultAsync(p => p.ProductType == ProductType.RawMaterial &&
                                                                  (p.ProductCode.ToLower() == lowerSearchTerm || p.Name.ToLower().Contains(lowerSearchTerm)));
            }
        }

        public async Task<BillOfMaterials> GetBomByIdAsync(int bomId)
        {
            using (var db = CreateDbContext())
            {
                return await db.BillOfMaterials
                                 .Include(b => b.BillOfMaterialsItems)
                                 .ThenInclude(i => i.RawMaterial)
                                 .FirstOrDefaultAsync(b => b.Id == bomId);
            }
        }

        public async Task SaveBomAsync(BillOfMaterials bom)
        {
            using (var db = CreateDbContext())
            {
                if (bom.Id > 0) // وضع التعديل
                {
                    // جلب قائمة البنود الحالية لحذفها قبل إضافة البنود المحدثة
                    var existingItems = await db.BillOfMaterialsItems.Where(i => i.BillOfMaterialsId == bom.Id).ToListAsync();
                    db.BillOfMaterialsItems.RemoveRange(existingItems);

                    // تحديث الكيان الرئيسي
                    db.BillOfMaterials.Update(bom);
                }
                else // وضع الإضافة
                {
                    db.BillOfMaterials.Add(bom);
                }
                // سيتم حفظ التغييرات باستخدام آلية التدقيق الموجودة في DatabaseContext
                await db.SaveChangesAsync();
            }
        }
    }
}