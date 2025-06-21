// GoodMorningFactory/UI/Views/AddEditAccountWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditAccountWindow : Window
    {
        public AddEditAccountWindow(int? accountId = null)
        {
            InitializeComponent();
            DataContext = new AddEditAccountViewModel(accountId);
        }
    }
}