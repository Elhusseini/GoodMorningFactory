// UI/Views/AddEditCurrencyWindow.xaml.cs
// *** الكود الكامل والنهائي - تم إضافة السطر المفقود ***
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditCurrencyWindow : Window
    {
        public AddEditCurrencyWindow(int? currencyId = null)
        {
            InitializeComponent();
            // ======================= بداية الإصلاح الرئيسي =======================
            // هذا السطر يقوم بإنشاء الـ ViewModel وربطه بالواجهة
            DataContext = new AddEditCurrencyViewModel(currencyId);
            // ======================== نهاية الإصلاح الرئيسي ========================
        }
    }
}