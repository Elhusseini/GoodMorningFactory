using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Interaction logic for AddEditSalesOrderWindow.xaml
    /// تم إعادة هيكلتها بالكامل لتعمل بنمط MVVM.
    /// </summary>
    public partial class AddEditSalesOrderWindow : Window
    {
        public AddEditSalesOrderWindow(int? orderId = null, int? sourceQuotationId = null)
        {
            InitializeComponent();
            // إنشاء ViewModel وتمرير المعرفات إليه، وتعيينه كمصدر بيانات للنافذة.
            DataContext = new AddEditSalesOrderViewModel(orderId, sourceQuotationId);
        }
    }
}
