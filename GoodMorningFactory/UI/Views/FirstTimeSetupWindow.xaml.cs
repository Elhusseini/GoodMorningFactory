// GoodMorning/UI/Views/FirstTimeSetupWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة الإعداد الأولي للنظام ***
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class FirstTimeSetupWindow : Window
    {
        public FirstTimeSetupWindow()
        {
            InitializeComponent();
            PasswordBox.Focus();
        }

        private void CreateAdminButton_Click(object sender, RoutedEventArgs e)
        {
            // التحقق من أن كلمة المرور ليست فارغة
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("يرجى إدخال كلمة المرور.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // التحقق من تطابق كلمتي المرور
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("كلمتا المرور غير متطابقتين.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var db = new DatabaseContext())
                {
                    // 1. التأكد من وجود دور "مسؤول النظام" أو إنشائه
                    var adminRole = db.Roles.FirstOrDefault(r => r.Name == "مسؤول النظام");
                    if (adminRole == null)
                    {
                        adminRole = new Role { Name = "مسؤول النظام", Description = "يمتلك جميع صلاحيات النظام." };
                        db.Roles.Add(adminRole);
                        db.SaveChanges(); // حفظ الدور للحصول على هوية
                    }

                    // 2. إنشاء المستخدم المدير
                    var adminUser = new User
                    {
                        Username = "admin",
                        PasswordHash = PasswordHelper.HashPassword(PasswordBox.Password),
                        RoleId = adminRole.Id,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        FirstName = "المدير",
                        LastName = "العام"
                    };
                    db.Users.Add(adminUser);
                    db.SaveChanges();

                    MessageBox.Show("تم إنشاء حساب المدير العام بنجاح. سيتم الآن الانتقال إلى شاشة تسجيل الدخول.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                    // إغلاق النافذة بنجاح
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ غير متوقع أثناء إنشاء حساب المدير: {ex.Message}", "خطأ فادح", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
