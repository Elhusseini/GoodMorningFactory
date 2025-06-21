// UI/Views/AddEditUserWindow.xaml.cs
// *** تحديث: تمت إضافة قواعد تحقق متقدمة عند حفظ المستخدم ***
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditUserWindow : Window
    {
        private User _userToEdit;
        private byte[] _userImageBytes;

        public AddEditUserWindow(User user = null)
        {
            InitializeComponent();
            LoadRolesAndDepartments();
            _userToEdit = user;

            if (_userToEdit != null)
            {
                Title = "تعديل مستخدم";
                LoadUserData();
            }
            else
            {
                Title = "إضافة مستخدم جديد";
                IsActiveCheckBox.IsChecked = true;
                DisplayImage();
            }
        }

        private void LoadRolesAndDepartments()
        {
            using (var db = new DatabaseContext())
            {
                RoleComboBox.ItemsSource = db.Roles.ToList();
                DepartmentComboBox.ItemsSource = db.Departments.ToList();
            }
        }

        private void LoadUserData()
        {
            UsernameTextBox.Text = _userToEdit.Username;
            UsernameTextBox.IsEnabled = false;
            FirstNameTextBox.Text = _userToEdit.FirstName;
            LastNameTextBox.Text = _userToEdit.LastName;
            EmailTextBox.Text = _userToEdit.Email;
            PhoneNumberTextBox.Text = _userToEdit.PhoneNumber;
            RoleComboBox.SelectedValue = _userToEdit.RoleId;
            DepartmentComboBox.SelectedValue = _userToEdit.DepartmentId;
            IsActiveCheckBox.IsChecked = _userToEdit.IsActive;
            _userImageBytes = _userToEdit.ProfilePicture;
            DisplayImage();
        }

        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "ملفات الصور (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg",
                Title = "اختر صورة شخصية"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _userImageBytes = File.ReadAllBytes(openFileDialog.FileName);
                    DisplayImage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تحميل الصورة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DisplayImage()
        {
            BitmapImage image = null;
            if (_userImageBytes != null && _userImageBytes.Length > 0)
            {
                image = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(_userImageBytes))
                {
                    stream.Position = 0;
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                }
            }
            else
            {
                try
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imagePath = Path.Combine(baseDirectory, "Assets", "default-user.png");
                    if (File.Exists(imagePath))
                    {
                        image = new BitmapImage();
                        image.BeginInit();
                        image.UriSource = new Uri(imagePath);
                        image.EndInit();
                    }
                }
                catch { }
            }
            if (image != null) { image.Freeze(); }
            ProfileImage.ImageSource = image;
        }

        // --- بداية التحديث: دالة الحفظ مع قواعد التحقق ---
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            #region Validation Logic
            // التحقق من الحقول الإلزامية
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("يرجى ملء جميع الحقول التي تحمل علامة النجمة (*).", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // التحقق من كلمة المرور عند الإضافة
            if (_userToEdit == null && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("كلمة المرور مطلوبة عند إنشاء مستخدم جديد.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // التحقق من تطابق كلمتي المرور
            if (!string.IsNullOrWhiteSpace(PasswordBox.Password) && PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("كلمتا المرور غير متطابقتين.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // التحقق من تنسيق البريد الإلكتروني
            try
            {
                var addr = new System.Net.Mail.MailAddress(EmailTextBox.Text);
                if (addr.Address != EmailTextBox.Text.Trim())
                {
                    throw new Exception();
                }
            }
            catch
            {
                MessageBox.Show("البريد الإلكتروني المدخل غير صالح.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            #endregion

            using (var db = new DatabaseContext())
            {
                #region Uniqueness Validation
                // التحقق من فرادة اسم المستخدم والبريد الإلكتروني
                if (_userToEdit == null) // حالة الإضافة
                {
                    if (db.Users.Any(u => u.Username.ToLower() == UsernameTextBox.Text.ToLower()))
                    {
                        MessageBox.Show("اسم المستخدم هذا موجود بالفعل.", "بيانات مكررة", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (db.Users.Any(u => u.Email.ToLower() == EmailTextBox.Text.ToLower()))
                    {
                        MessageBox.Show("هذا البريد الإلكتروني مستخدم بالفعل.", "بيانات مكررة", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else // حالة التعديل
                {
                    if (db.Users.Any(u => u.Id != _userToEdit.Id && u.Email.ToLower() == EmailTextBox.Text.ToLower()))
                    {
                        MessageBox.Show("هذا البريد الإلكتروني مستخدم من قبل مستخدم آخر.", "بيانات مكررة", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                #endregion

                #region Save Logic
                if (_userToEdit == null)
                {
                    _userToEdit = new User { CreatedAt = DateTime.Now };
                    db.Users.Add(_userToEdit);
                }
                else
                {
                    db.Users.Attach(_userToEdit);
                }

                _userToEdit.Username = UsernameTextBox.Text;
                _userToEdit.RoleId = (int)RoleComboBox.SelectedValue;
                _userToEdit.DepartmentId = (int?)DepartmentComboBox.SelectedValue;
                _userToEdit.IsActive = IsActiveCheckBox.IsChecked ?? false;
                _userToEdit.FirstName = FirstNameTextBox.Text;
                _userToEdit.LastName = LastNameTextBox.Text;
                _userToEdit.Email = EmailTextBox.Text;
                _userToEdit.PhoneNumber = PhoneNumberTextBox.Text;
                _userToEdit.ProfilePicture = _userImageBytes;

                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    _userToEdit.PasswordHash = PasswordHelper.HashPassword(PasswordBox.Password);
                }

                db.SaveChanges();
                #endregion
            }
            this.DialogResult = true;
        }
        // --- نهاية التحديث ---
    }
}
