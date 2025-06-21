// GoodMorning/UI/Views/InventoryCountsView.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// تم تحويل هذه الواجهة بالكامل لتعمل بنمط MVVM.
    /// الكود الخلفي الآن لا يحتوي على أي منطق برمجي.
    /// </summary>
    public partial class InventoryCountsView : UserControl
    {
        public InventoryCountsView()
        {
            InitializeComponent();
            // تعيين الـ ViewModel الجديد كمصدر بيانات للواجهة
            DataContext = new InventoryCountsViewModel();
        }
    }
}