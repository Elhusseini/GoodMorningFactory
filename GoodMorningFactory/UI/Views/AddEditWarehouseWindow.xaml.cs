// UI/Views/AddEditWarehouseWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إضافة وتعديل المخازن ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditWarehouseWindow : Window
    {
        private int? _warehouseId;

        public AddEditWarehouseWindow(int? warehouseId = null)
        {
            InitializeComponent();
            _warehouseId = warehouseId;
            if (_warehouseId.HasValue) { LoadWarehouseData(); }
        }

        private void LoadWarehouseData()
        {
            using (var db = new DatabaseContext())
            {
                var warehouse = db.Warehouses.Find(_warehouseId.Value);
                if (warehouse != null)
                {
                    CodeTextBox.Text = warehouse.Code;
                    NameTextBox.Text = warehouse.Name;
                    AddressTextBox.Text = warehouse.Address;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeTextBox.Text) || string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("كود واسم المخزن حقول مطلوبة.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                Warehouse warehouse;
                if (_warehouseId.HasValue) { warehouse = db.Warehouses.Find(_warehouseId.Value); }
                else { warehouse = new Warehouse(); db.Warehouses.Add(warehouse); }

                warehouse.Code = CodeTextBox.Text;
                warehouse.Name = NameTextBox.Text;
                warehouse.Address = AddressTextBox.Text;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}