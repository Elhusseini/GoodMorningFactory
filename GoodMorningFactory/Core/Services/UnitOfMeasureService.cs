// GoodMorningFactory/Core/Services/UnitOfMeasureService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة وحدات القياس ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class UnitOfMeasureService : IUnitOfMeasureService
    {
        public async Task<List<UnitOfMeasure>> GetUomsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.UnitsOfMeasure.OrderBy(u => u.Name).ToListAsync();
            }
        }

        public async Task<UnitOfMeasure> GetUomByIdAsync(int uomId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.UnitsOfMeasure.FindAsync(uomId);
            }
        }

        public async Task SaveUomAsync(UnitOfMeasure uom)
        {
            using (var db = new DatabaseContext())
            {
                if (uom.Id == 0) // إضافة جديدة
                {
                    db.UnitsOfMeasure.Add(uom);
                }
                else // تحديث
                {
                    db.UnitsOfMeasure.Update(uom);
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteUomAsync(int uomId)
        {
            using (var db = new DatabaseContext())
            {
                // التحقق من عدم استخدام الوحدة في أي منتج
                bool isUsed = await db.Products.AnyAsync(p => p.UnitOfMeasureId == uomId);
                if (isUsed)
                {
                    throw new InvalidOperationException("لا يمكن حذف وحدة القياس لأنها مستخدمة في منتجات حالية.");
                }

                var uomToDelete = await db.UnitsOfMeasure.FindAsync(uomId);
                if (uomToDelete != null)
                {
                    db.UnitsOfMeasure.Remove(uomToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}