// GoodMorningFactory/Core/Services/SetupService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة الإعداد الأولي ***
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class SetupService : ISetupService
    {
        public async Task CreateAdminUserAsync(string adminPassword)
        {
            using (var db = new DatabaseContext())
            {
                // 1. التأكد من وجود دور "مسؤول النظام" أو إنشائه
                var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "مسؤول النظام");
                if (adminRole == null)
                {
                    adminRole = new Role { Name = "مسؤول النظام", Description = "يمتلك جميع صلاحيات النظام." };
                    db.Roles.Add(adminRole);
                    await db.SaveChangesAsync(); // حفظ الدور للحصول على هوية
                }

                // 2. إنشاء المستخدم المدير
                var adminUser = new User
                {
                    Username = "admin",
                    PasswordHash = PasswordHelper.HashPassword(adminPassword),
                    RoleId = adminRole.Id,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    FirstName = "المدير",
                    LastName = "العام",
                    Email = "admin@example.com" // بريد إلكتروني افتراضي
                };
                db.Users.Add(adminUser);
                await db.SaveChangesAsync();
            }
        }
    }
}