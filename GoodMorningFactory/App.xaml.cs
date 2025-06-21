// App.xaml.cs
// *** تحديث: إضافة منطق التحقق من أول تشغيل للنظام ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
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
                // التأكد من إنشاء قاعدة البيانات وملفاتها
                using (var db = new DatabaseContext())
                {
                    db.Database.EnsureCreated();
                }

                // تهيئة الصلاحيات الأساسية في كل مرة لضمان وجودها
                SeedInitialData();
                AppSettings.LoadSettings();

                // --- بداية التعديل: منطق التحقق من أول تشغيل ---
                bool isFirstRun;
                using (var db = new DatabaseContext())
                {
                    // التحقق مما إذا كان جدول المستخدمين فارغاً
                    isFirstRun = !db.Users.Any();
                }

                if (isFirstRun)
                {
                    // هذه هي المرة الأولى، أظهر نافذة الإعداد الأولي
                    var setupWindow = new FirstTimeSetupWindow();
                    if (setupWindow.ShowDialog() == true)
                    {
                        // بعد نجاح الإعداد، استمر في فتح شاشة تسجيل الدخول
                        ShowLoginAndMainWindow();
                    }
                    else
                    {
                        // إذا أغلق المستخدم نافذة الإعداد، أغلق التطبيق
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    // النظام تم إعداده مسبقاً، أظهر شاشة تسجيل الدخول مباشرة
                    ShowLoginAndMainWindow();
                }
                // --- نهاية التعديل ---
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ فادح عند بدء تشغيل البرنامج:\n\n{ex.Message}\n\n{ex.InnerException?.Message}", "خطأ في بدء التشغيل", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        // دالة مجمعة لفتح نافذة تسجيل الدخول ثم النافذة الرئيسية
        private void ShowLoginAndMainWindow()
        {
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            PerformAutoBackup();
            base.OnExit(e);
        }

        private void PerformAutoBackup()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var companyInfo = db.CompanyInfos.FirstOrDefault();
                    if (companyInfo == null || !companyInfo.IsAutoBackupEnabled)
                    {
                        return; // الخروج إذا كانت الميزة معطلة
                    }

                    string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GoodMorningFactory");
                    string dbPath = Path.Combine(appDataFolder, "GoodMorningFactory.db");
                    string backupFolder = Path.Combine(appDataFolder, "Backups");

                    if (!File.Exists(dbPath)) return; // لا يوجد ما يتم نسخه

                    // 1. إنشاء نسخة احتياطية جديدة
                    string backupFileName = $"AutoBackup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db";
                    string backupFilePath = Path.Combine(backupFolder, backupFileName);
                    File.Copy(dbPath, backupFilePath, true);

                    // 2. حذف النسخ القديمة
                    var backupFiles = new DirectoryInfo(backupFolder)
                        .GetFiles("AutoBackup_*.db")
                        .OrderByDescending(f => f.CreationTime)
                        .ToList();

                    if (backupFiles.Count > companyInfo.BackupsToKeep)
                    {
                        var filesToDelete = backupFiles.Skip(companyInfo.BackupsToKeep);
                        foreach (var file in filesToDelete)
                        {
                            file.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // في حالة حدوث خطأ، لا نعرض رسالة للمستخدم حتى لا نزعجه عند الإغلاق
                // يمكن تسجيل الخطأ في ملف log للمراجعة لاحقاً
                System.Diagnostics.Debug.WriteLine($"Auto-backup failed: {ex.Message}");
            }
        }

        private void SeedInitialData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // تهيئة الصلاحيات الأساسية
                    var allDefinedPermissions = PermissionSeeder.GetAllPermissions();
                    var existingPermissionNames = db.Permissions.Select(p => p.Name).ToHashSet();

                    var missingPermissions = allDefinedPermissions
                        .Where(p => !existingPermissionNames.Contains(p.Name))
                        .ToList();

                    if (missingPermissions.Any())
                    {
                        db.Permissions.AddRange(missingPermissions);
                        db.SaveChanges();
                    }

                    // لا نقم بإنشاء المستخدم أو الدور هنا، سيتم إنشاؤه في نافذة الإعداد الأولي
                }
            }
            catch (Exception ex)
            {
                throw new Exception("فشل في تهيئة البيانات الأولية للنظام.", ex);
            }
        }
    }
}
