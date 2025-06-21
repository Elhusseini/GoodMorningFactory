using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddShipmentWindow : Window
    {
        public AddShipmentWindow(int salesOrderId)
        {
            InitializeComponent();
            DataContext = new AddShipmentViewModel(salesOrderId);
        }
    }
}
