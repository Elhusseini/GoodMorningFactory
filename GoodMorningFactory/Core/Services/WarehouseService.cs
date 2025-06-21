// GoodMorningFactory/Core/Services/WarehouseService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class WarehouseService : IWarehouseService
    {
        public async Task<List<Warehouse>> GetWarehousesAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Warehouses.OrderBy(w => w.Name).ToListAsync();
            }
        }

        public async Task<Warehouse> GetWarehouseByIdAsync(int warehouseId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Warehouses.FindAsync(warehouseId);
            }
        }

        public async Task SaveWarehouseAsync(Warehouse warehouse)
        {
            using (var db = new DatabaseContext())
            {
                if (warehouse.Id == 0)
                {
                    db.Warehouses.Add(warehouse);
                }
                else
                {
                    db.Warehouses.Update(warehouse);
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<StorageLocation>> GetLocationsForWarehouseAsync(int warehouseId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.StorageLocations
                               .Where(l => l.WarehouseId == warehouseId)
                               .OrderBy(l => l.Name)
                               .ToListAsync();
            }
        }

        public async Task<StorageLocation> GetLocationByIdAsync(int locationId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.StorageLocations.FindAsync(locationId);
            }
        }

        // --- بداية التعديل الجذري: تطبيق منطق الموقع الافتراضي بطريقة أكثر قوة ---
        public async Task SaveLocationAsync(StorageLocation location)
        {
            using (var db = new DatabaseContext())
            {
                // الخطوة 1: التحقق من أن الكود فريد داخل نفس المخزن
                if (await db.StorageLocations.AnyAsync(l => l.WarehouseId == location.WarehouseId && l.Code == location.Code && l.Id != location.Id))
                {
                    throw new InvalidOperationException("هذا الكود مستخدم بالفعل في موقع آخر بنفس المخزن.");
                }

                // الخطوة 2: إذا كان الموقع الحالي سيصبح هو الافتراضي
                if (location.IsDefault)
                {
                    // ابحث عن أي مواقع أخرى افتراضية في نفس المخزن وقم بإلغاء تحديدها
                    var otherDefaults = await db.StorageLocations
                        .Where(l => l.WarehouseId == location.WarehouseId && l.IsDefault && l.Id != location.Id)
                        .ToListAsync();

                    foreach (var other in otherDefaults)
                    {
                        other.IsDefault = false;
                    }
                }

                // الخطوة 3: حفظ الموقع الحالي (إضافة أو تعديل)
                if (location.Id == 0)
                {
                    // حالة الإضافة
                    db.StorageLocations.Add(location);
                }
                else
                {
                    // حالة التعديل: جلب الكيان من قاعدة البيانات ثم تحديثه
                    var locationInDb = await db.StorageLocations.FindAsync(location.Id);
                    if (locationInDb != null)
                    {
                        locationInDb.Name = location.Name;
                        locationInDb.Code = location.Code;
                        locationInDb.Description = location.Description;
                        locationInDb.IsActive = location.IsActive;
                        locationInDb.IsDefault = location.IsDefault;
                    }
                }
                await db.SaveChangesAsync();
            }
        }
        // --- نهاية التعديل ---

        public async Task DeleteLocationAsync(int locationId)
        {
            using (var db = new DatabaseContext())
            {
                bool hasInventory = await db.Inventories.AnyAsync(i => i.StorageLocationId == locationId && i.Quantity > 0);
                if (hasInventory)
                {
                    throw new InvalidOperationException("لا يمكن حذف هذا الموقع لوجود مخزون به. يرجى نقل المخزون أولاً.");
                }

                var locationToDelete = await db.StorageLocations.FindAsync(locationId);
                if (locationToDelete != null)
                {
                    db.StorageLocations.Remove(locationToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}