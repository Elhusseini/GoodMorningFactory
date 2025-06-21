// GoodMorningFactory/UI/ViewModels/CategoryViewModel.cs
using GoodMorningFactory.Data.Models;
using System.Collections.ObjectModel;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل عقدة واحدة (فئة) في شجرة الفئات.
    /// </summary>
    public class CategoryViewModel : BaseViewModel
    {
        public Category Category { get; }
        public ObservableCollection<CategoryViewModel> Children { get; }
        public int ProductCount { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        // خاصية لعرض اسم الفئة مع عدد المنتجات بداخلها
        public string DisplayName => $"{Category.Name} ({ProductCount})";

        public CategoryViewModel(Category category, int productCount)
        {
            Category = category;
            Children = new ObservableCollection<CategoryViewModel>();
            ProductCount = productCount;
        }
    }
}
