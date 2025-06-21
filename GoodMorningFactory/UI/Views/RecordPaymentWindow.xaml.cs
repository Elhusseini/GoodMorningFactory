using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class RecordPaymentWindow : Window
    {
        public RecordPaymentWindow(int saleId)
        {
            InitializeComponent();

            ISalesService salesService = new SalesService();
            var viewModel = new RecordPaymentViewModel(salesService, saleId);

            this.DataContext = viewModel;
        }
    }
}
