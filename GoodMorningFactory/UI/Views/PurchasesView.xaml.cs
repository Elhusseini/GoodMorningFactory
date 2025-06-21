using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Interaction logic for PurchasesView.xaml
    /// </summary>
    public partial class PurchasesView : UserControl
    {
        public PurchasesView()
        {
            InitializeComponent();
            DataContext = new PurchasesViewModel();
        }
    }
}