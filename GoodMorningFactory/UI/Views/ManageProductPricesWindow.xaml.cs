using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class ManageProductPricesWindow : Window
    {
        public ManageProductPricesWindow(int priceListId)
        {
            InitializeComponent();
            var service = new PriceListService();
            DataContext = new ManageProductPricesViewModel(service, priceListId);
        }
    }
}