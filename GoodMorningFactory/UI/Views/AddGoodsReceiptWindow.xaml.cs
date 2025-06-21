// GoodMorningFactory/UI/Views/AddGoodsReceiptWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddGoodsReceiptWindow : Window
    {
        public AddGoodsReceiptWindow(int purchaseOrderId)
        {
            InitializeComponent();
            // تعيين الـ ViewModel ليكون مصدر البيانات والمنطق لهذه النافذة
            DataContext = new AddGoodsReceiptViewModel(purchaseOrderId);
        }
    }
}