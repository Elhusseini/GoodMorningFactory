// GoodMorningFactory/Core/Services/CostCenterService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة مراكز التكلفة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class CostCenterService : ICostCenterService
    {
        public async Task<List<CostCenter>> GetCostCentersAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.CostCenters.OrderBy(c => c.Name).ToListAsync();
            }
        }

        public async Task<CostCenter> GetCostCenterByIdAsync(int id)
        {
            using (var db = new DatabaseContext())
            {
                return await db.CostCenters.FindAsync(id);
            }
        }

        public async Task SaveCostCenterAsync(CostCenter costCenter)
        {
            using (var db = new DatabaseContext())
            {
                if (costCenter.Id == 0)
                {
                    db.CostCenters.Add(costCenter);
                }
                else
                {
                    db.CostCenters.Update(costCenter);
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteCostCenterAsync(int id)
        {
            using (var db = new DatabaseContext())
            {
                // *** تطوير: منطق الحذف الآمن ***
                // التحقق مما إذا كان مركز التكلفة مستخدماً في أي قيد يومية
                bool isUsed = await db.JournalVoucherItems.AnyAsync(jvi => jvi.CostCenterId == id);
                if (isUsed)
                {
                    // إذا كان مستخدماً، نمنع الحذف ونرسل رسالة خطأ واضحة
                    throw new InvalidOperationException("لا يمكن حذف مركز التكلفة هذا لأنه مرتبط بقيود يومية حالية.");
                }

                var costCenterToDelete = await db.CostCenters.FindAsync(id);
                if (costCenterToDelete != null)
                {
                    db.CostCenters.Remove(costCenterToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}