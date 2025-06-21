// GoodMorningFactory/UI/Views/AdjustStockWindow.xaml.cs
// *** الكود الكامل والمعدل - أصبح نظيفاً تماماً ***
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AdjustStockWindow : Window
    {
        public AdjustStockWindow()
        {
            InitializeComponent();
            DataContext = new AdjustStockViewModel();
        }
    }
}