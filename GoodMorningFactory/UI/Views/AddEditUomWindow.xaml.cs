// GoodMorningFactory/UI/Views/AddEditUomWindow.xaml.cs
// *** الكود الكامل والمعدل - أصبح نظيفاً تماماً ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditUomWindow : Window
    {
        public AddEditUomWindow(int? uomId = null)
        {
            InitializeComponent();
            var service = new UnitOfMeasureService();
            DataContext = new AddEditUomViewModel(service, uomId);
        }
    }
}