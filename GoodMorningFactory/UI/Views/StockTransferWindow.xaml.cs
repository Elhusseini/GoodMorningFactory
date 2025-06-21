using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class StockTransferWindow : Window
    {
        public StockTransferWindow()
        {
            InitializeComponent();
            DataContext = new StockTransferViewModel();
        }
    }
}