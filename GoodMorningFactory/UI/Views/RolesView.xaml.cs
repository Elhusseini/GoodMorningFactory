// UI/Views/RolesView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة الأدوار ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class RolesView : UserControl
    {
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
                    RolesDataGrid.ItemsSource = db.Roles.ToList();
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل الأدوار: {ex.Message}"); }
        }

        private void AddRoleButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditRoleWindow();
            if (addWindow.ShowDialog() == true) { LoadRoles(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Role role)
            {
                var editWindow = new AddEditRoleWindow(role.Id);
                if (editWindow.ShowDialog() == true) { LoadRoles(); }
            }
        }

        private void ManagePermissionsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Role role)
            {
                var permissionsWindow = new ManagePermissionsWindow(role.Id);
                permissionsWindow.ShowDialog();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) { /* ... */ }
    }
}