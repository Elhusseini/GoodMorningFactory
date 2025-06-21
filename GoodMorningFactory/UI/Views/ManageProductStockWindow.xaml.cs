// GoodMorningFactory/UI/Views/ManageProductStockWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class ManageProductStockWindow : Window
    {
        public ManageProductStockWindow(ProductViewModel product)
        {
            InitializeComponent();
            DataContext = new ManageProductStockViewModel(product);
        }
    }
}