// UI/Views/AddEditCategoryWindow.xaml.cs
// *** الكود الكامل للكود الخلفي لنافذة إضافة وتعديل الفئات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditCategoryWindow : Window
    {
        private int? _categoryId;

        public AddEditCategoryWindow(int? categoryId = null)
        {
            InitializeComponent();
            _categoryId = categoryId;
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            using (var db = new DatabaseContext())
            {
                // تحميل الفئات المتاحة لتكون "فئة أم"
                ParentCategoryComboBox.ItemsSource = db.Categories.Where(c => c.Id != _categoryId).ToList(); // منع اختيار الفئة لنفسها
            }

            if (_categoryId.HasValue)
            {
                using (var db = new DatabaseContext())
                {
                    var category = db.Categories.Find(_categoryId.Value);
                    if (category != null)
                    {
                        NameTextBox.Text = category.Name;
                        DescriptionTextBox.Text = category.Description;
                        ParentCategoryComboBox.SelectedValue = category.ParentCategoryId;
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("اسم الفئة حقل مطلوب.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                Category category;
                if (_categoryId.HasValue) { category = db.Categories.Find(_categoryId.Value); }
                else { category = new Category(); db.Categories.Add(category); }

                category.Name = NameTextBox.Text;
                category.Description = DescriptionTextBox.Text;
                category.ParentCategoryId = (int?)ParentCategoryComboBox.SelectedValue;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}