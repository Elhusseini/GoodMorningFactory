// UI/Views/LoginWindow.xaml.cs
// *** تحديث: تم إضافة تعيين المستخدم الحالي واستدعاء تحميل الصلاحيات فوراً ***
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var user = db.Users.Include(u => u.Role).FirstOrDefault(u => u.Username.ToLower() == UsernameTextBox.Text.ToLower());

                    if (user != null && user.IsActive && PasswordHelper.VerifyPassword(PasswordBox.Password, user.PasswordHash))
                    {
                        // --- بداية الإصلاح الجذري ---
                        // 1. تعيين المستخدم الحالي في الخدمة المركزية
                        CurrentUserService.LoggedInUser = user;

                        // 2. تحميل صلاحيات هذا المستخدم فوراً
                        PermissionsService.LoadUserPermissions(user.RoleId);
                        // --- نهاية الإصلاح ---

                        this.DialogResult = true; // سيؤدي هذا إلى إغلاق نافذة تسجيل الدخول
                    }
                    else
                    {
                        MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة, أو أن الحساب غير نشط.", "خطأ في تسجيل الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء محاولة تسجيل الدخول: {ex.Message}", "خطأ فادح", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}