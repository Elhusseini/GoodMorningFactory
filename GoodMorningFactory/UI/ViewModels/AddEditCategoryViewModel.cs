// GoodMorningFactory/UI/ViewModels/AddEditCategoryViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditCategoryViewModel : BaseViewModel
    {
        private readonly ICategoryService _categoryService;
        private readonly int? _categoryId;
        private readonly int? _parentId;

        #region Properties
        private string _windowTitle;
        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(); }
        }

        private string _categoryCode;
        public string CategoryCode { get => _categoryCode; set { _categoryCode = value; OnPropertyChanged(); } }

        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
        }

        private string _categoryDescription;
        public string CategoryDescription
        {
            get => _categoryDescription;
            set { _categoryDescription = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Category> _parentCategories;
        public ObservableCollection<Category> ParentCategories
        {
            get => _parentCategories;
            set { _parentCategories = value; OnPropertyChanged(); }
        }

        private Category _selectedParentCategory;
        public Category SelectedParentCategory
        {
            get => _selectedParentCategory;
            set { _selectedParentCategory = value; OnPropertyChanged(); }
        }
        #endregion

        // مُنشئ فارغ لوقت التصميم
        public AddEditCategoryViewModel() { }

        public AddEditCategoryViewModel(ICategoryService categoryService, int? categoryId, int? parentId)
        {
            _categoryService = categoryService;
            _categoryId = categoryId;
            _parentId = parentId;
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var possibleParents = await _categoryService.GetPossibleParentCategoriesAsync(_categoryId);
                possibleParents.Insert(0, new Category { Id = 0, Name = "(بدون أب)" });
                ParentCategories = new ObservableCollection<Category>(possibleParents);

                if (_categoryId.HasValue) // وضع التعديل
                {
                    WindowTitle = "تعديل فئة";
                    var category = await _categoryService.GetCategoryByIdAsync(_categoryId.Value);
                    if (category != null)
                    {
                        CategoryCode = category.CategoryCode;
                        CategoryName = category.Name;
                        CategoryDescription = category.Description;
                        SelectedParentCategory = ParentCategories.FirstOrDefault(p => p.Id == category.ParentCategoryId) ?? ParentCategories.First();
                    }
                }
                else // وضع الإضافة
                {
                    WindowTitle = "إضافة فئة جديدة";
                    SelectedParentCategory = ParentCategories.FirstOrDefault(p => p.Id == _parentId) ?? ParentCategories.First();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(CategoryName) || string.IsNullOrWhiteSpace(CategoryCode))
            {
                MessageBox.Show("اسم الفئة وكود الفئة حقول مطلوبة.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                int? selectedParentId = (SelectedParentCategory?.Id == 0) ? null : SelectedParentCategory?.Id;

                if (_categoryId.HasValue) // تحديث
                {
                    var category = await _categoryService.GetCategoryByIdAsync(_categoryId.Value);
                    category.CategoryCode = CategoryCode.Trim().ToUpper();
                    category.Name = CategoryName.Trim();
                    category.Description = CategoryDescription?.Trim();
                    category.ParentCategoryId = selectedParentId;
                    await _categoryService.UpdateCategoryAsync(category);
                }
                else // إضافة
                {
                    var newCategory = new Category
                    {
                        CategoryCode = CategoryCode.Trim().ToUpper(),
                        Name = CategoryName.Trim(),
                        Description = CategoryDescription?.Trim(),
                        ParentCategoryId = selectedParentId
                    };
                    await _categoryService.AddCategoryAsync(newCategory);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}