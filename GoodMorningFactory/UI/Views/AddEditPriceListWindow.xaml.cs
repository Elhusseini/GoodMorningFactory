using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditPriceListWindow : Window
    {
        public AddEditPriceListWindow(int? priceListId = null)
        {
            InitializeComponent();
            var service = new PriceListService();
            DataContext = new AddEditPriceListViewModel(service, priceListId);
        }
    }
}