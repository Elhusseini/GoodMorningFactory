// GoodMorningFactory/UI/Views/DepartmentsView.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Code-behind لواجهة عرض الأقسام.
    /// دوره يقتصر على تهيئة الواجهة وربطها بالـ ViewModel المقابل.
    /// </summary>
    public partial class DepartmentsView : UserControl
    {
        public DepartmentsView()
        {
            InitializeComponent();
            // إنشاء وربط الـ ViewModel مع الواجهة
            DataContext = new DepartmentsViewViewModel();
        }
    }
}
