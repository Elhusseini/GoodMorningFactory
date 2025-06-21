using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Interaction logic for CreateInvoiceFromOrderWindow.xaml
    /// </summary>
    public partial class CreateInvoiceFromOrderWindow : Window
    {
        public CreateInvoiceFromOrderWindow(int salesOrderId)
        {
            InitializeComponent();
            DataContext = new CreateInvoiceFromOrderViewModel(salesOrderId);
        }
    }
}
