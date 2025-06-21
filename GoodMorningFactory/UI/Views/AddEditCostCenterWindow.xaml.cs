// UI/Views/AddEditCostCenterWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إضافة/تعديل مركز تكلفة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditCostCenterWindow : Window
    {
        private readonly int? _costCenterId;

        public AddEditCostCenterWindow(int? costCenterId = null)
        {
            InitializeComponent();
            _costCenterId = costCenterId;
            if (_costCenterId.HasValue)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            using (var db = new DatabaseContext())
            {
                var center = db.CostCenters.Find(_costCenterId);
                if (center != null)
                {
                    NameTextBox.Text = center.Name;
                    DescriptionTextBox.Text = center.Description;
                    IsActiveCheckBox.IsChecked = center.IsActive;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("اسم مركز التكلفة حقل مطلوب.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                CostCenter center;
                if (_costCenterId.HasValue)
                {
                    center = db.CostCenters.Find(_costCenterId.Value);
                }
                else
                {
                    center = new CostCenter();
                    db.CostCenters.Add(center);
                }

                center.Name = NameTextBox.Text;
                center.Description = DescriptionTextBox.Text;
                center.IsActive = IsActiveCheckBox.IsChecked ?? true;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}