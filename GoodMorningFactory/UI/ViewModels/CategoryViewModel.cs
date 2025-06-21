// UI/ViewModels/CategoryViewModel.cs
// *** ملف جديد: ViewModel لعرض الفئات بشكل شجري ***
using GoodMorningFactory.Data.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class CategoryViewModel : INotifyPropertyChanged
    {
        public Category Category { get; set; }
        public ObservableCollection<CategoryViewModel> Children { get; set; }
        public int ProductCount { get; set; }

        public string DisplayName => $"{Category.Name} ({ProductCount})";

        public CategoryViewModel(Category category, int productCount)
        {
            Category = category;
            Children = new ObservableCollection<CategoryViewModel>();
            ProductCount = productCount;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}