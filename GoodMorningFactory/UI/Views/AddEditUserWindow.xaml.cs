// UI/Views/AddEditUserWindow.xaml.cs
// الكود الخلفي لنافذة إضافة وتعديل المستخدمين
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditUserWindow : Window
    {
        private User _userToEdit;

        public AddEditUserWindow(User user = null)
        {
            InitializeComponent();
            LoadRoles();
            _userToEdit = user;
            if (_userToEdit != null) // وضع التعديل
            {
                Title = "تعديل مستخدم";
                UsernameTextBox.Text = _userToEdit.Username;
                RoleComboBox.SelectedValue = _userToEdit.RoleId;
                IsActiveCheckBox.IsChecked = _userToEdit.IsActive;
            }
            else // وضع الإضافة
            {
                Title = "إضافة مستخدم جديد";
                IsActiveCheckBox.IsChecked = true;
            }
        }

        private void LoadRoles()
        {
            using (var db = new DatabaseContext())
            {
                RoleComboBox.ItemsSource = db.Roles.ToList();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("اسم المستخدم والدور حقول مطلوبة.", "بيانات غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // في وضع الإضافة، كلمة المرور مطلوبة
            if (_userToEdit == null && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("كلمة المرور مطلوبة عند إنشاء مستخدم جديد.", "بيانات غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            {
                if (_userToEdit == null) // إضافة
                {
                    var newUser = new User
                    {
                        Username = UsernameTextBox.Text,
                        PasswordHash = PasswordHelper.HashPassword(PasswordBox.Password),
                        RoleId = (int)RoleComboBox.SelectedValue,
                        IsActive = IsActiveCheckBox.IsChecked ?? false
                    };
                    db.Users.Add(newUser);
                }
                else // تعديل
                {
                    _userToEdit.Username = UsernameTextBox.Text;
                    _userToEdit.RoleId = (int)RoleComboBox.SelectedValue;
                    _userToEdit.IsActive = IsActiveCheckBox.IsChecked ?? false;

                    // تحديث كلمة المرور فقط إذا قام المستخدم بإدخال قيمة جديدة
                    if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                    {
                        _userToEdit.PasswordHash = PasswordHelper.HashPassword(PasswordBox.Password);
                    }
                    db.Users.Update(_userToEdit);
                }
                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}
