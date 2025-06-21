// GoodMorningFactory/UI/Views/AddPurchaseReturnWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddPurchaseReturnWindow : Window
    {
        /// <summary>
        /// يُستخدم عند الضغط على "إضافة مرتجع جديد".
        /// </summary>
        public AddPurchaseReturnWindow()
        {
            InitializeComponent();
            DataContext = new AddPurchaseReturnViewModel(null);
        }

        /// <summary>
        /// يُستخدم عند إنشاء مرتجع من فاتورة محددة.
        /// </summary>
        public AddPurchaseReturnWindow(int purchaseId)
        {
            InitializeComponent();
            DataContext = new AddPurchaseReturnViewModel(purchaseId);
        }
    }
}