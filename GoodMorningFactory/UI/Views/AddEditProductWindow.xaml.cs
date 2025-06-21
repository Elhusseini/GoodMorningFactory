using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditProductWindow : Window
    {
        public AddEditProductWindow(int? productId = null, int? sourceProductIdToCopy = null)
        {
            InitializeComponent();
            DataContext = new AddEditProductViewModel(productId, sourceProductIdToCopy);
        }
    }
}