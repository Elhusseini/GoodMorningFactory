using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditPurchaseInvoiceWindow : Window
    {
        public AddEditPurchaseInvoiceWindow(int? purchaseId = null, int? purchaseOrderId = null, int? grnId = null)
        {
            InitializeComponent();
            DataContext = new AddEditPurchaseInvoiceViewModel(purchaseId, purchaseOrderId, grnId);
        }
    }
}