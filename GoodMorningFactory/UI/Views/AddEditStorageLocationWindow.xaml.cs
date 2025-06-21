// UI/Views/AddEditStorageLocationWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إضافة وتعديل موقع فرعي ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditStorageLocationWindow : Window
    {
        private readonly int _warehouseId;
        private readonly int? _locationId;

        public AddEditStorageLocationWindow(int warehouseId, int? locationId = null)
        {
            InitializeComponent();
            _warehouseId = warehouseId;
            _locationId = locationId;

            if (_locationId.HasValue)
            {
                WindowTitle.Text = "تعديل موقع تخزين";
                LoadLocationData();
            }
            else
            {
                IsActiveCheckBox.IsChecked = true;
            }
        }

        private void LoadLocationData()
        {
            using (var db = new DatabaseContext())
            {
                var location = db.StorageLocations.Find(_locationId.Value);
                if (location != null)
                {
                    CodeTextBox.Text = location.Code;
                    NameTextBox.Text = location.Name;
                    DescriptionTextBox.Text = location.Description;
                    IsActiveCheckBox.IsChecked = location.IsActive;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(CodeTextBox.Text))
            {
                MessageBox.Show("كود واسم الموقع حقول مطلوبة.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                // التحقق من أن الكود فريد داخل نفس المخزن
                if (db.StorageLocations.Any(l => l.WarehouseId == _warehouseId && l.Code == CodeTextBox.Text && l.Id != _locationId))
                {
                    MessageBox.Show("هذا الكود مستخدم بالفعل في موقع آخر بنفس المخزن.", "كود مكرر");
                    return;
                }

                StorageLocation location;
                if (_locationId.HasValue)
                {
                    location = db.StorageLocations.Find(_locationId.Value);
                }
                else
                {
                    location = new StorageLocation { WarehouseId = _warehouseId };
                    db.StorageLocations.Add(location);
                }

                location.Code = CodeTextBox.Text;
                location.Name = NameTextBox.Text;
                location.Description = DescriptionTextBox.Text;
                location.IsActive = IsActiveCheckBox.IsChecked ?? true;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}
