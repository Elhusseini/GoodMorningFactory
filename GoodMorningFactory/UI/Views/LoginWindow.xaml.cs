// UI/Views/LoginWindow.xaml.cs
// *** تحديث: تم إصلاح منطق التحقق من الدخول ***
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using Microsoft.EntityFrameworkCore;
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
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("يرجى إدخال اسم المستخدم وكلمة المرور.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            {
                var userFromDb = db.Users.FirstOrDefault(u => u.Username.ToLower() == UsernameTextBox.Text.ToLower());

                if (userFromDb == null)
                {
                    MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة.", "فشل الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!userFromDb.IsActive)
                {
                    MessageBox.Show("هذا الحساب غير نشط. يرجى مراجعة المسؤول.", "فشل الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (PasswordHelper.VerifyPassword(PasswordBox.Password, userFromDb.PasswordHash))
                {
                    // *** بداية التصحيح: إعادة تحميل المستخدم مع دوره بشكل كامل ***
                    CurrentUserService.LoggedInUser = db.Users.Include(u => u.Role).SingleOrDefault(u => u.Id == userFromDb.Id);
                    // *** نهاية التصحيح ***
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة.", "فشل الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}