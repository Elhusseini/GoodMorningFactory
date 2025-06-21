// UI/Views/ManagePermissionsWindow.xaml.cs
// *** تحديث: تمت إضافة وضع القراءة فقط لتعطيل التعديل عند عرض الصلاحيات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class ManagePermissionsWindow : Window
    {
        private readonly int _roleId;
        private ObservableCollection<PermissionGroupViewModel> _permissionGroups = new ObservableCollection<PermissionGroupViewModel>();

        // --- بداية التحديث: تعديل المُنشئ ليقبل متغير isReadOnly ---
        public ManagePermissionsWindow(int roleId, bool isReadOnly = false)
        {
            InitializeComponent();
            _roleId = roleId;
            DataContext = this;
            LoadPermissions();

            if (isReadOnly)
            {
                SetupReadOnlyMode();
            }
            else
            {
                LoadRolesForCopy();
            }
        }
        // --- نهاية التحديث ---

        public ObservableCollection<PermissionGroupViewModel> PermissionGroups
        {
            get { return _permissionGroups; }
        }

        private void LoadPermissions()
        {
            using (var db = new DatabaseContext())
            {
                var role = db.Roles.Find(_roleId);
                if (role == null) { this.Close(); return; }
                RoleNameTextBlock.Text = $"صلاحيات الدور: {role.Name}";

                var allPermissions = db.Permissions.ToList();
                var rolePermissionIds = db.RolePermissions
                                          .Where(rp => rp.RoleId == _roleId)
                                          .Select(rp => rp.PermissionId)
                                          .ToHashSet();

                var groupedByModule = allPermissions.GroupBy(p => p.Module);

                foreach (var group in groupedByModule)
                {
                    var groupVm = new PermissionGroupViewModel(group.Key);
                    foreach (var perm in group)
                    {
                        var permVm = new ViewModels.PermissionViewModel
                        {
                            Id = perm.Id,
                            Description = perm.Description,
                            IsSelected = rolePermissionIds.Contains(perm.Id),
                            Parent = groupVm
                        };
                        groupVm.Permissions.Add(permVm);
                    }
                    groupVm.VerifyCheckState();
                    _permissionGroups.Add(groupVm);
                }

                PermissionsTreeView.ItemsSource = _permissionGroups;
            }
        }

        // --- بداية الإضافة: دوال جديدة لوضع القراءة فقط ونسخ الصلاحيات ---
        private void SetupReadOnlyMode()
        {
            Title = "عرض الصلاحيات";
            PermissionsTreeView.IsEnabled = false; // تعطيل الشجرة بالكامل
            CopySection.Visibility = Visibility.Collapsed; // إخفاء قسم النسخ
            SaveButton.Visibility = Visibility.Collapsed; // إخفاء زر الحفظ
            CancelButton.Content = "إغلاق"; // تغيير نص زر الإلغاء
        }

        private void LoadRolesForCopy()
        {
            using (var db = new DatabaseContext())
            {
                CopyFromRoleComboBox.ItemsSource = db.Roles.Where(r => r.Id != _roleId).ToList();
                CopyFromRoleComboBox.DisplayMemberPath = "Name";
                CopyFromRoleComboBox.SelectedValuePath = "Id";
            }
        }

        private void CopyPermissions_Click(object sender, RoutedEventArgs e)
        {
            if (CopyFromRoleComboBox.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار دور لنسخ الصلاحيات منه أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("سيتم استبدال الصلاحيات المحددة حالياً بصلاحيات الدور المختار. هل أنت متأكد؟", "تأكيد النسخ", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                return;
            }

            int sourceRoleId = (int)CopyFromRoleComboBox.SelectedValue;

            using (var db = new DatabaseContext())
            {
                var sourcePermissionIds = db.RolePermissions
                                            .Where(rp => rp.RoleId == sourceRoleId)
                                            .Select(rp => rp.PermissionId)
                                            .ToHashSet();

                foreach (var group in _permissionGroups)
                {
                    foreach (var perm in group.Permissions)
                    {
                        perm.IsSelected = sourcePermissionIds.Contains(perm.Id);
                    }
                }
            }
        }
        // --- نهاية الإضافة ---

        private void SavePermissions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var existingPermissions = db.RolePermissions.Where(rp => rp.RoleId == _roleId);
                    if (existingPermissions.Any())
                    {
                        db.RolePermissions.RemoveRange(existingPermissions);
                    }

                    foreach (var group in _permissionGroups)
                    {
                        foreach (var perm in group.Permissions.Where(p => p.IsSelected))
                        {
                            db.RolePermissions.Add(new RolePermission { RoleId = _roleId, PermissionId = perm.Id });
                        }
                    }

                    db.SaveChanges();
                    MessageBox.Show("تم حفظ الصلاحيات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الصلاحيات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
