using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class CustomerStatementWindow : Window
    {
        public CustomerStatementWindow(int customerId)
        {
            InitializeComponent();

            // إنشاء الخدمات
            ICustomerService customerService = new CustomerService();
            IPrintingService printingService = new PrintingService(); // *** إنشاء خدمة الطباعة ***

            // تمرير الخدمات إلى الـ ViewModel
            var viewModel = new CustomerStatementViewModel(customerService, printingService, customerId);

            // تعيين DataContext
            this.DataContext = viewModel;
        }
    }
}
