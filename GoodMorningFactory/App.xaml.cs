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
    /// <summary>
    /// نقطة الدخول الرئيسية للتطبيق. تدير دورة حياة التطبيق وتقوم بتهيئة المكونات الأساسية.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// يتم استدعاء هذه الدالة عند بدء التطبيق. تقوم بتهيئة قاعدة البيانات والخدمات الأساسية.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // تهيئة الخدمات المركزية للتطبيق (مثل خدمات المصادقة والصلاحيات)
                AppServices.Initialize();

                // إنشاء قاعدة البيانات إذا لم تكن موجودة
                using (var db = new DatabaseContext())
                {
                    db.Database.EnsureCreated();
                }

                // تهيئة البيانات الأساسية مثل الصلاحيات
                SeedInitialData();
                // تحميل إعدادات التطبيق
                AppSettings.LoadSettings();

                // التحقق مما إذا كان هذا أول تشغيل للتطبيق
                bool isFirstRun;
                using (var db = new DatabaseContext())
                {
                    isFirstRun = !db.Users.Any();
                }

                if (isFirstRun)
                {
                    // عرض نافذة الإعداد الأولي للتطبيق
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
                    ShowLoginAndMainWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ فادح عند بدء تشغيل البرنامج:\n\n{ex.Message}\n\n{ex.InnerException?.Message}", 
                              "خطأ في بدء التشغيل", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// تعرض نافذة تسجيل الدخول ثم النافذة الرئيسية بعد نجاح تسجيل الدخول
        /// </summary>
        private void ShowLoginAndMainWindow()
        {
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                var mainWindow = new MainWindow();
                this.MainWindow = mainWindow;
                mainWindow.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// يتم استدعاؤها عند إغلاق التطبيق. تقوم بإجراء النسخ الاحتياطي التلقائي إذا كان مفعلاً
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            PerformAutoBackup();
            base.OnExit(e);
        }

        /// <summary>
        /// تنفيذ النسخ الاحتياطي التلقائي لقاعدة البيانات وفقاً لإعدادات الشركة
        /// </summary>
        private void PerformAutoBackup()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // التحقق من تفعيل خاصية النسخ الاحتياطي التلقائي
                    var companyInfo = db.CompanyInfos.FirstOrDefault();
                    if (companyInfo == null || !companyInfo.IsAutoBackupEnabled)
                    {
                        return; // الخروج إذا كانت الميزة معطلة
                    }

                    // تحديد مسارات الملفات
                    string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GoodMorningFactory");
                    string dbPath = Path.Combine(appDataFolder, "GoodMorningFactory.db");
                    string backupFolder = Path.Combine(appDataFolder, "Backups");

                    if (!Directory.Exists(backupFolder))
                    {
                        Directory.CreateDirectory(backupFolder);
                    }

                    if (!File.Exists(dbPath)) return; // لا يوجد ما يتم نسخه

                    // إنشاء نسخة احتياطية جديدة
                    string backupFileName = $"AutoBackup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db";
                    string backupFilePath = Path.Combine(backupFolder, backupFileName);
                    File.Copy(dbPath, backupFilePath, true);

                    // إدارة النسخ الاحتياطية القديمة
                    var backupFiles = new DirectoryInfo(backupFolder)
                        .GetFiles("AutoBackup_*.db")
                        .OrderByDescending(f => f.CreationTime)
                        .ToList();

                    // حذف النسخ الزائدة عن العدد المحدد في الإعدادات
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
                System.Diagnostics.Debug.WriteLine($"Auto-backup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// تهيئة البيانات الأساسية في قاعدة البيانات مثل الصلاحيات
        /// </summary>
        private void SeedInitialData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // تهيئة الصلاحيات الأساسية في النظام
                    var allDefinedPermissions = PermissionSeeder.GetAllPermissions();
                    var existingPermissionNames = db.Permissions.Select(p => p.Name).ToHashSet();

                    // إضافة الصلاحيات الجديدة فقط
                    var missingPermissions = allDefinedPermissions
                        .Where(p => !existingPermissionNames.Contains(p.Name))
                        .ToList();

                    if (missingPermissions.Any())
                    {
                        db.Permissions.AddRange(missingPermissions);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("فشل في تهيئة البيانات الأولية للنظام.", ex);
            }
        }
    }
}