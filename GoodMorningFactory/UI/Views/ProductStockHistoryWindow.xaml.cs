// GoodMorningFactory/UI/Views/ProductStockHistoryWindow.xaml.cs
// *** الكود الكامل والنهائي - أصبح نظيفاً تماماً ***
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class ProductStockHistoryWindow : Window
    {
        public ProductStockHistoryWindow(int productId)
        {
            InitializeComponent();
            DataContext = new ProductStockHistoryViewModel(productId);
        }
    }
}