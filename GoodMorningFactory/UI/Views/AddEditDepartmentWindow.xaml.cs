// GoodMorningFactory/UI/Views/AddEditDepartmentWindow.xaml.cs
// *** الكود الكامل والنهائي - أصبح نظيفاً تماماً ***
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditDepartmentWindow : Window
    {
        public AddEditDepartmentWindow(int? departmentId = null)
        {
            InitializeComponent();
            DataContext = new AddEditDepartmentViewModel(departmentId);
        }
    }
}