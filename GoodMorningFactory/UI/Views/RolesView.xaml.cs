using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    // ViewModel لعرض بيانات الدور مع عدد المستخدمين
    public class RoleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int UserCount { get; set; }
    }

    public partial class RolesView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public RolesView()
        {
            InitializeComponent();
            LoadRoles();
        }

        private void LoadRoles()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.Roles.AsQueryable();

                    // تطبيق فلتر البحث
                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(r => r.Name.ToLower().Contains(searchText) || r.Description.ToLower().Contains(searchText));
                    }

                    _totalItems = query.Count();

                    // جلب بيانات الصفحة الحالية
                    var rolesForPage = query.OrderBy(r => r.Name)
                                            .Skip((_currentPage - 1) * _pageSize)
                                            .Take(_pageSize)
                                            .ToList();

                    // تحويل الأدوار إلى ViewModel مع حساب عدد المستخدمين لكل دور
                    var roleViewModels = rolesForPage.Select(role => new RoleViewModel
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Description = role.Description,
                        UserCount = db.Users.Count(u => u.RoleId == role.Id)
                    }).ToList();

                    RolesDataGrid.ItemsSource = roleViewModels;
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الأدوار: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages == 0) totalPages = 1;
            PageInfoTextBlock.Text = $"الصفحة {_currentPage} من {totalPages} (إجمالي الأدوار: {_totalItems})";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < totalPages;
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1) { _currentPage--; LoadRoles(); }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (_currentPage < totalPages) { _currentPage++; LoadRoles(); }
        }

        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _currentPage = 1;
                LoadRoles();
            }
        }

        private void AddRoleButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditRoleWindow();
            if (addWindow.ShowDialog() == true) { LoadRoles(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is RoleViewModel role)
            {
                var editWindow = new AddEditRoleWindow(role.Id);
                if (editWindow.ShowDialog() == true) { LoadRoles(); }
            }
        }

        private void ManagePermissionsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is RoleViewModel role)
            {
                var permissionsWindow = new ManagePermissionsWindow(role.Id);
                permissionsWindow.ShowDialog();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is RoleViewModel roleToDelete)
            {
                if (roleToDelete.Name == "مسؤول النظام")
                {
                    MessageBox.Show("لا يمكن حذف دور المسؤول الرئيسي.", "عملية غير مسموحة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (roleToDelete.UserCount > 0)
                {
                    MessageBox.Show("لا يمكن حذف هذا الدور لوجود مستخدمين مرتبطين به. يرجى نقل المستخدمين إلى دور آخر أولاً.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"هل أنت متأكد من حذف الدور '{roleToDelete.Name}' بشكل نهائي؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            var role = db.Roles.Find(roleToDelete.Id);
                            if (role != null)
                            {
                                var permissions = db.RolePermissions.Where(rp => rp.RoleId == role.Id);
                                db.RolePermissions.RemoveRange(permissions);
                                db.Roles.Remove(role);
                                db.SaveChanges();
                                LoadRoles();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل حذف الدور: {ex.Message}", "خطأ");
                    }
                }
            }
        }
    }
}
