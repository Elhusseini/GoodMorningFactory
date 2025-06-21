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
                var categoryName = selectedCategory.Category.Name;
                var result = MessageBox.Show($"هل أنت متأكد من حذف الفئة '{categoryName}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        // تحقق من عدم وجود فئات فرعية
                        bool hasChildren = db.Categories.Any(c => c.ParentCategoryId == selectedCategory.Category.Id);
                        if (hasChildren)
                        {
                            MessageBox.Show("لا يمكن حذف الفئة لوجود فئات فرعية مرتبطة بها.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        // تحقق من عدم وجود منتجات مرتبطة
                        bool hasProducts = db.Products.Any(p => p.CategoryId == selectedCategory.Category.Id);
                        if (hasProducts)
                        {
                            MessageBox.Show("لا يمكن حذف الفئة لوجود منتجات مرتبطة بها.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        db.Categories.Remove(selectedCategory.Category);
                        db.SaveChanges();
                        LoadCategories();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل حذف الفئة: {ex.Message}");
                }
            }
            else { MessageBox.Show("يرجى تحديد فئة لحذفها."); }
        }
    }
}