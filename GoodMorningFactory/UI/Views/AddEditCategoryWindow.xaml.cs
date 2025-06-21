// GoodMorningFactory/UI/Views/AddEditCategoryWindow.xaml.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Code-behind لنافذة إضافة وتعديل الفئات.
    /// أصبح دوره يقتصر على تهيئة وعرض النافذة والتفاعل مع الـ ViewModel.
    /// </summary>
    public partial class AddEditCategoryWindow : Window
    {
        private readonly AddEditCategoryViewModel _viewModel;

        public AddEditCategoryWindow(int? categoryId = null, int? parentId = null)
        {
            InitializeComponent();
            // إنشاء ViewModel وتمرير الخدمة والمعرفات اللازمة
            _viewModel = new AddEditCategoryViewModel(new CategoryService(), categoryId, parentId);
            DataContext = _viewModel;
            // استدعاء تحميل البيانات عند تحميل النافذة
            Loaded += async (s, e) => await _viewModel.LoadDataAsync();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // استدعاء وظيفة الحفظ في الـ ViewModel
            bool success = await _viewModel.SaveAsync();
            if (success)
            {
                // إغلاق النافذة بنجاح فقط إذا تمت عملية الحفظ
                DialogResult = true;
            }
        }
    }
}
