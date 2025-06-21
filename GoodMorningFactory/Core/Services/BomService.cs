// GoodMorningFactory/Core/Services/BomService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة قوائم المكونات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class BomService : IBomService
    {
        public async Task<List<BillOfMaterials>> GetBomsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.BillOfMaterials.Include(b => b.FinishedGood).ToListAsync();
            }
        }

        public async Task DeleteBomAsync(int bomId)
        {
            using (var db = new DatabaseContext())
            {
                var bomToDelete = await db.BillOfMaterials.FindAsync(bomId);
                if (bomToDelete != null)
                {
                    // الحذف المتتالي (Cascade Delete) سيهتم بحذف البنود المرتبطة
                    db.BillOfMaterials.Remove(bomToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}