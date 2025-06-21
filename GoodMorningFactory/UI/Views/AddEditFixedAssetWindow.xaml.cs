// UI/Views/AddEditFixedAssetWindow.xaml.cs
// *** الكود الكامل والمعدل - أصبح نظيفاً ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditFixedAssetWindow : Window
    {
        public AddEditFixedAssetWindow(int? assetId = null)
        {
            InitializeComponent();
            var service = new FixedAssetService();
            DataContext = new AddEditFixedAssetViewModel(service, assetId);
        }
    }
}