// UI/Views/AddEditInventoryCountWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// تم تحويل هذه النافذة بالكامل لتعمل بنمط MVVM.
    /// الكود الخلفي الآن لا يحتوي على أي منطق.
    /// </summary>
    public partial class AddEditInventoryCountWindow : Window
    {
        public AddEditInventoryCountWindow(int? inventoryCountId = null)
        {
            InitializeComponent();
            DataContext = new AddEditInventoryCountViewModel(inventoryCountId);
        }
    }
}