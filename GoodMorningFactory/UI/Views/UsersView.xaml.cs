// UI/Views/UsersView.xaml.cs
// *** تحديث: تمت إضافة منطق لزر "عرض الصلاحيات" ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.Views
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; } // <-- إضافة جديدة
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public BitmapImage ProfilePicture { get; set; }
    }

    public partial class UsersView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalItems = 0;

        public UsersView()
        {
            InitializeComponent();
            LoadFilters();
            LoadUsers();
        }

        private void LoadFilters()
        {
            StatusFilterComboBox.ItemsSource = new List<object>
            {
                new { Name = "الكل", Value = (bool?)null },
                new { Name = "نشط", Value = (bool?)true },
                new { Name = "غير نشط", Value = (bool?)false }
            };
            StatusFilterComboBox.SelectedValuePath = "Value";
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                try
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imagePath = Path.Combine(baseDirectory, "Assets", "default-user.png");
                    if (File.Exists(imagePath))
                    {
                        return new BitmapImage(new Uri(imagePath));
                    }
                }
                catch { }
                return null;
            }

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void LoadUsers()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.Users.Include(u => u.Role).Include(u => u.Department).AsQueryable();

                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(u => u.Username.ToLower().Contains(searchText) ||
                                                 (u.FirstName + " " + u.LastName).ToLower().Contains(searchText) ||
                                                 u.Email.ToLower().Contains(searchText));
                    }

                    if (StatusFilterComboBox.SelectedValue is bool status)
                    {
                        query = query.Where(u => u.IsActive == status);
                    }

                    _totalItems = query.Count();

                    var users = query.OrderBy(u => u.Username)
                                     .Skip((_currentPage - 1) * _pageSize)
                                     .Take(_pageSize)
                                     .ToList()
                                     .Select(u => new UserViewModel
                                     {
                                         Id = u.Id,
                                         RoleId = u.RoleId, // <-- إضافة جديدة
                                         Username = u.Username,
                                         FullName = $"{u.FirstName} {u.LastName}".Trim(),
                                         Email = u.Email,
                                         RoleName = u.Role.Name,
                                         DepartmentName = u.Department?.Name ?? "N/A",
                                         IsActive = u.IsActive,
                                         CreatedAt = u.CreatedAt,
                                         ProfilePicture = LoadImage(u.ProfilePicture)
                                     }).ToList();

                    UsersDataGrid.ItemsSource = users;
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المستخدمين: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- بداية الإضافة: دالة زر عرض الصلاحيات ---
        private void ViewPermissionsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is UserViewModel userVm)
            {
                // فتح نافذة الصلاحيات في وضع القراءة فقط
                var permissionsWindow = new ManagePermissionsWindow(userVm.RoleId, true);
                permissionsWindow.ShowDialog();
            }
        }
        // --- نهاية الإضافة ---

        private void UpdatePageInfo()
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages == 0) totalPages = 1;
            PageInfoTextBlock.Text = $"الصفحة {_currentPage} من {totalPages} (إجمالي المستخدمين: {_totalItems})";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < totalPages;
        }
        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadUsers(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize); if (_currentPage < totalPages) { _currentPage++; LoadUsers(); } }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadUsers(); } }
        private void Filter_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { _currentPage = 1; LoadUsers(); } }
        private void AddUserButton_Click(object sender, RoutedEventArgs e) { var addWindow = new AddEditUserWindow(); if (addWindow.ShowDialog() == true) { LoadUsers(); } }
        private void EditButton_Click(object sender, RoutedEventArgs e) { if ((sender as Button)?.DataContext is UserViewModel userVm) { using (var db = new DatabaseContext()) { var userToEdit = db.Users.Find(userVm.Id); if (userToEdit != null) { var editWindow = new AddEditUserWindow(userToEdit); if (editWindow.ShowDialog() == true) { LoadUsers(); } } } } }
        private void ToggleStatus_Click(object sender, RoutedEventArgs e) { if ((sender as Button)?.DataContext is UserViewModel userVm) { if (userVm.Username.ToLower() == "admin") { MessageBox.Show("لا يمكن تغيير حالة حساب المسؤول الرئيسي.", "عملية غير مسموحة", MessageBoxButton.OK, MessageBoxImage.Warning); return; } string action = userVm.IsActive ? "تعطيل" : "تفعيل"; var result = MessageBox.Show($"هل أنت متأكد من {action} حساب المستخدم '{userVm.Username}'؟", "تأكيد الإجراء", MessageBoxButton.YesNo, MessageBoxImage.Question); if (result == MessageBoxResult.Yes) { using (var db = new DatabaseContext()) { var user = db.Users.Find(userVm.Id); if (user != null) { user.IsActive = !user.IsActive; db.SaveChanges(); LoadUsers(); } } } } }
    }
}
