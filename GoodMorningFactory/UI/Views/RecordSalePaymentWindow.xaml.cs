// UI/Views/RecordSalePaymentWindow.xaml.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class RecordSalePaymentWindow : Window
    {
        // تم التعديل لاستخدام ViewModel بدلاً من الكود اليدوي
        public RecordSalePaymentWindow(int saleId)
        {
            InitializeComponent();

            // إنشاء نسخة من خدمة المبيعات وتمريرها للـ ViewModel
            ISalesService salesService = new SalesService();
            var viewModel = new RecordPaymentViewModel(salesService, saleId);

            // تعيين الـ ViewModel كمصدر بيانات للنافذة
            this.DataContext = viewModel;
        }
    }
}