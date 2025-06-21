// UI/Views/InventoryDashboardView.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// الكود الخلفي الآن نظيف ومسؤول فقط عن تعيين الـ DataContext.
    /// </summary>
    public partial class InventoryDashboardView : UserControl
    {
        public InventoryDashboardView()
        {
            InitializeComponent();
            // كل المنطق انتقل إلى InventoryDashboardViewModel
            DataContext = new InventoryDashboardViewModel();
        }
    }
}