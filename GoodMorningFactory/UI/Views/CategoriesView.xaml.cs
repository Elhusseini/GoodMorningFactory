// UI/Views/CategoriesView.xaml.cs
// *** الكود الكامل للكود الخلفي لواجهة إدارة الفئات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class CategoriesView : UserControl
    {
        public CategoriesView()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var categories = db.Categories.Include(c => c.Products).ToList();
                    var categoryViewModels = categories.Select(c => new CategoryViewModel(c, c.Products.Count)).ToList();

                    var dictionary = categoryViewModels.ToDictionary(vm => vm.Category.Id);
                    var rootCategories = new ObservableCollection<CategoryViewModel>();

                    foreach (var vm in categoryViewModels)
                    {
                        if (vm.Category.ParentCategoryId.HasValue && dictionary.ContainsKey(vm.Category.ParentCategoryId.Value))
                        {
                            dictionary[vm.Category.ParentCategoryId.Value].Children.Add(vm);
                        }
                        else
                        {
                            rootCategories.Add(vm);
                        }
                    }
                    CategoriesTreeView.ItemsSource = rootCategories;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الفئات: {ex.Message}");
            }
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditCategoryWindow();
            if (addWindow.ShowDialog() == true) { LoadCategories(); }
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesTreeView.SelectedItem is CategoryViewModel selectedCategory)
            {
                var editWindow = new AddEditCategoryWindow(selectedCategory.Category.Id);
                if (editWindow.ShowDialog() == true) { LoadCategories(); }
            }
            else { MessageBox.Show("يرجى تحديد فئة لتعديلها."); }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesTreeView.SelectedItem is CategoryViewModel selectedCategory)
            {
                // (منطق الحذف مع التحقق من عدم وجود فئات فرعية أو منتجات مرتبطة)
                MessageBox.Show($"سيتم هنا حذف الفئة: {selectedCategory.Category.Name}", "تحت الإنشاء");
            }
            else { MessageBox.Show("يرجى تحديد فئة لحذفها."); }
        }
    }
}