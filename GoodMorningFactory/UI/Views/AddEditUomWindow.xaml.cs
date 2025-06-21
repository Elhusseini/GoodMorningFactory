// UI/Views/AddEditUomWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إضافة وتعديل وحدات القياس ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditUomWindow : Window
    {
        private int? _uomId;

        public AddEditUomWindow(int? uomId = null)
        {
            InitializeComponent();
            _uomId = uomId;
            if (_uomId.HasValue) { LoadUomData(); }
        }

        private void LoadUomData()
        {
            using (var db = new DatabaseContext())
            {
                var uom = db.UnitsOfMeasure.Find(_uomId.Value);
                if (uom != null)
                {
                    NameTextBox.Text = uom.Name;
                    AbbreviationTextBox.Text = uom.Abbreviation;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(AbbreviationTextBox.Text))
            {
                MessageBox.Show("اسم الوحدة والاختصار حقول مطلوبة.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                UnitOfMeasure uom;
                if (_uomId.HasValue) { uom = db.UnitsOfMeasure.Find(_uomId.Value); }
                else { uom = new UnitOfMeasure(); db.UnitsOfMeasure.Add(uom); }

                uom.Name = NameTextBox.Text;
                uom.Abbreviation = AbbreviationTextBox.Text;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}