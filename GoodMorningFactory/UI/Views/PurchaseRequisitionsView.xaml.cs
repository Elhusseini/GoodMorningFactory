using System.Windows.Controls;
using GoodMorningFactory.UI.ViewModels;

namespace GoodMorningFactory.UI.Views
{
    public partial class PurchaseRequisitionsView : UserControl
    {
        public PurchaseRequisitionsView()
        {
            InitializeComponent();
            // The ViewModel now handles all the logic.
            // The DataContext is set in the XAML file.
        }
    }
}
