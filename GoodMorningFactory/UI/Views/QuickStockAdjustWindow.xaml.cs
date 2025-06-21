// GoodMorningFactory/UI/Views/QuickStockAdjustWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class QuickStockAdjustWindow : Window
    {
        public QuickStockAdjustWindow(int productId, int storageLocationId)
        {
            InitializeComponent();
            DataContext = new QuickStockAdjustViewModel(productId, storageLocationId);
        }
    }
}