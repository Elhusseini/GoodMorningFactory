// UI/Views/AddEditPriceListWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إضافة وتعديل قوائم الأسعار ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditPriceListWindow : Window
    {
        private int? _priceListId;

        public AddEditPriceListWindow(int? priceListId = null)
        {
            InitializeComponent();
            _priceListId = priceListId;
            if (_priceListId.HasValue) { LoadPriceListData(); }
        }

        private void LoadPriceListData()
        {
            using (var db = new DatabaseContext())
            {
                var priceList = db.PriceLists.Find(_priceListId.Value);
                if (priceList != null)
                {
                    NameTextBox.Text = priceList.Name;
                    DescriptionTextBox.Text = priceList.Description;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("اسم القائمة حقل مطلوب.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                PriceList priceList;
                if (_priceListId.HasValue) { priceList = db.PriceLists.Find(_priceListId.Value); }
                else { priceList = new PriceList(); db.PriceLists.Add(priceList); }

                priceList.Name = NameTextBox.Text;
                priceList.Description = DescriptionTextBox.Text;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}