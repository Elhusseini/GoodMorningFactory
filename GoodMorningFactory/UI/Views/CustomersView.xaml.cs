using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Interaction logic for CustomersView.xaml
    /// </summary>
    public partial class CustomersView : UserControl
    {
        public CustomersView()
        {
            InitializeComponent();
            // Set the DataContext to our new ViewModel.
            // All data binding and commands will now be handled by the ViewModel.
            DataContext = new CustomersViewViewModel();
        }
    }
}
