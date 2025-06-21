// UI/Views/ManagePermissionsWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إدارة الصلاحيات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class ManagePermissionsWindow : Window
    {
        private readonly int _roleId;
        private List<PermissionViewModel> _permissionTree = new List<PermissionViewModel>();

        public ManagePermissionsWindow(int roleId)
        {
            InitializeComponent();
            _roleId = roleId;
            LoadPermissions();
        }

        private void LoadPermissions()
        {
            using (var db = new DatabaseContext())
            {
                var role = db.Roles.Find(_roleId);
                if (role == null) { this.Close(); return; }
                RoleNameTextBlock.Text = $"صلاحيات الدور: {role.Name}";

                var allPermissions = db.Permissions.ToList();
                var rolePermissions = db.RolePermissions.Where(rp => rp.RoleId == _roleId).Select(rp => rp.PermissionId).ToList();

                var grouped = allPermissions.GroupBy(p => p.Module);

                foreach (var group in grouped)
                {
                    var moduleNode = new PermissionViewModel(group.Key, false);
                    foreach (var permission in group)
                    {
                        moduleNode.Children.Add(new PermissionViewModel(permission.Name, rolePermissions.Contains(permission.Id)) { PermissionId = permission.Id });
                    }
                    _permissionTree.Add(moduleNode);
                }
                PermissionsTreeView.ItemsSource = _permissionTree;
            }
        }

        private void SavePermissions_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            {
                var existingPermissions = db.RolePermissions.Where(rp => rp.RoleId == _roleId);
                db.RolePermissions.RemoveRange(existingPermissions);

                foreach (var moduleNode in _permissionTree)
                {
                    foreach (var permissionNode in moduleNode.Children)
                    {
                        if (permissionNode.IsSelected)
                        {
                            db.RolePermissions.Add(new RolePermission { RoleId = _roleId, PermissionId = permissionNode.PermissionId });
                        }
                    }
                }
                db.SaveChanges();
                MessageBox.Show("تم حفظ الصلاحيات بنجاح.");
                this.Close();
            }
        }
    }
}