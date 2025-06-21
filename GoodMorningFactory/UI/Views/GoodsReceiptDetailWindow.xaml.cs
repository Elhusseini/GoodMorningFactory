// GoodMorningFactory/UI/Views/GoodsReceiptDetailWindow.xaml.cs
// *** الكود الكامل والنهائي - أصبح نظيفاً تماماً ***
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class GoodsReceiptDetailWindow : Window
    {
        public GoodsReceiptDetailWindow(int grnId)
        {
            InitializeComponent();
            DataContext = new GoodsReceiptDetailViewModel(grnId);
        }
    }
}