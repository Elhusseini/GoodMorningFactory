// App.xaml.cs
// *** تحديث: تم استدعاء دالة تحميل الصلاحيات بعد تسجيل الدخول ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                SeedInitialData();

#if DEBUG
                using (var db = new DatabaseContext())
                {
                    CurrentUserService.LoggedInUser = db.Users.Include(u => u.Role).FirstOrDefault(u => u.Username == "admin");
                    if (CurrentUserService.LoggedInUser != null)
                    {
                        PermissionsService.LoadUserPermissions(CurrentUserService.LoggedInUser.RoleId); // <-- إضافة جديدة
                    }
                }
                var mainWindow = new MainWindow();
                mainWindow.Show();
#else
                var loginWindow = new LoginWindow();
                if (loginWindow.ShowDialog() == true)
                {
                    // بعد نجاح تسجيل الدخول، يتم تحميل صلاحيات المستخدم
                    PermissionsService.LoadUserPermissions(CurrentUserService.LoggedInUser.RoleId); // <-- إضافة جديدة
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                }
                else
                {
                    Application.Current.Shutdown();
                }
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ فادح عند بدء تشغيل البرنامج:\n\n{ex.Message}\n\n{ex.InnerException?.Message}", "خطأ في بدء التشغيل", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        // --- دالة مبسطة ومقاومة للأخطاء لإنشاء المستخدم الافتراضي فقط ---
        private void SeedInitialData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // التأكد من إنشاء قاعدة البيانات والجداول
                    db.Database.EnsureCreated();

                    // --- التحقق من وجود المستخدم "admin" وإنشاؤه إذا لم يكن موجوداً ---
                    if (!db.Users.Any(u => u.Username.ToLower() == "admin"))
                    {
                        // جلب دور المسؤول الذي تم إنشاؤه تلقائياً
                        var adminRole = db.Roles.FirstOrDefault(r => r.Name == "مسؤول النظام");
                        if (adminRole == null)
                        {
                            // في حالة عدم وجود الدور، يتم إنشاؤه
                            adminRole = new Role { Name = "مسؤول النظام" };
                            db.Roles.Add(adminRole);
                            db.SaveChanges();
                        }

                        // إنشاء المستخدم
                        var adminUser = new User
                        {
                            Username = "admin",
                            PasswordHash = Core.Helpers.PasswordHelper.HashPassword("12345"),
                            RoleId = adminRole.Id,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };
                        db.Users.Add(adminUser);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("فشل في تهيئة المستخدم الافتراضي للنظام.", ex);
            }
        }
    }
}
