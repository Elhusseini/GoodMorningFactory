// GoodMorningFactory/UI/Views/RunDepreciationWindow.xaml.cs
// *** الكود الكامل والمعدل - أصبح نظيفاً ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class RunDepreciationWindow : Window
    {
        public RunDepreciationWindow()
        {
            InitializeComponent();
            var service = new FixedAssetService();
            DataContext = new RunDepreciationViewModel(service);
        }
    }
}