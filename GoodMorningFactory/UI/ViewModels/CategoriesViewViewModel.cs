// GoodMorningFactory/UI/ViewModels/CategoriesViewViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel الرئيسي لواجهة عرض الفئات الشجرية.
    /// </summary>
    public class CategoriesViewViewModel : BaseViewModel
    {
        private readonly ICategoryService _categoryService;

        private ObservableCollection<CategoryViewModel> _categoriesTree;
        public ObservableCollection<CategoryViewModel> CategoriesTree
        {
            get => _categoriesTree;
            set { _categoriesTree = value; OnPropertyChanged(); }
        }

        private CategoryViewModel _selectedCategory;
        public CategoryViewModel SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SelectedItemChangedCommand { get; }

        public CategoriesViewViewModel()
        {
            _categoryService = new CategoryService();

            AddCategoryCommand = new RelayCommand(AddCategory);
            EditCategoryCommand = new RelayCommand(EditCategory, CanEditOrDelete);
            DeleteCategoryCommand = new RelayCommand(DeleteCategory, CanEditOrDelete);
            RefreshCommand = new RelayCommand(async _ => await LoadCategoriesAsync());
            SelectedItemChangedCommand = new RelayCommand(OnSelectedItemChanged);

            LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                CategoriesTree = await _categoryService.GetCategoryTreeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الفئات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSelectedItemChanged(object selectedItem)
        {
            SelectedCategory = selectedItem as CategoryViewModel;
        }

        private void AddCategory(object parameter)
        {
            // عند الإضافة، يمكن تمرير الفئة المحددة كـ "أب" افتراضي
            int? parentId = SelectedCategory?.Category.Id;
            var addWindow = new AddEditCategoryWindow(null, parentId);
            if (addWindow.ShowDialog() == true)
            {
                LoadCategoriesAsync();
            }
        }

        private void EditCategory(object parameter)
        {
            var editWindow = new AddEditCategoryWindow(SelectedCategory.Category.Id);
            if (editWindow.ShowDialog() == true)
            {
                LoadCategoriesAsync();
            }
        }

        private async void DeleteCategory(object parameter)
        {
            var categoryName = SelectedCategory.Category.Name;
            var result = MessageBox.Show($"هل أنت متأكد من حذف الفئة '{categoryName}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _categoryService.DeleteCategoryAsync(SelectedCategory.Category.Id);
                SelectedCategory = null; // إلغاء التحديد بعد الحذف
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ في الحذف", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedCategory != null;
        }
    }
}
