// GoodMorningFactory/UI/Views/AddEditRoleWindow.xaml.cs
// *** The Complete and Final Corrected Code ***
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditRoleWindow : Window
    {
        public AddEditRoleWindow(int? roleId = null)
        {
            InitializeComponent();
            DataContext = new AddEditRoleViewModel(roleId);
        }
    }
}