using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Interaction logic for ProductPurchaseHistoryWindow.xaml
    /// </summary>
    public partial class ProductPurchaseHistoryWindow : Window
    {
        public ProductPurchaseHistoryWindow(int productId)
        {
            InitializeComponent();
            DataContext = new ProductPurchaseHistoryViewModel(productId);
        }
    }
}