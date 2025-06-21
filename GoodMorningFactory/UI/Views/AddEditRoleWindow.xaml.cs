// UI/Views/AddEditRoleWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إضافة وتعديل دور ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditRoleWindow : Window
    {
        private int? _roleId;

        public AddEditRoleWindow(int? roleId = null)
        {
            InitializeComponent();
            _roleId = roleId;
            if (_roleId.HasValue) { LoadRoleData(); }
        }

        private void LoadRoleData()
        {
            using (var db = new DatabaseContext())
            {
                var role = db.Roles.Find(_roleId.Value);
                if (role != null)
                {
                    NameTextBox.Text = role.Name;
                    DescriptionTextBox.Text = role.Description;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text)) { MessageBox.Show("اسم الدور حقل مطلوب."); return; }
            using (var db = new DatabaseContext())
            {
                Role role;
                if (_roleId.HasValue) { role = db.Roles.Find(_roleId.Value); }
                else { role = new Role(); db.Roles.Add(role); }

                role.Name = NameTextBox.Text;
                role.Description = DescriptionTextBox.Text;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}