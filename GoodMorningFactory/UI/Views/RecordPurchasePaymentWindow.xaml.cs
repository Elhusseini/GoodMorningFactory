using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class RecordPurchasePaymentWindow : Window
    {
        public RecordPurchasePaymentWindow(int purchaseId)
        {
            InitializeComponent();
            DataContext = new RecordPurchasePaymentViewModel(purchaseId);
        }
    }
}