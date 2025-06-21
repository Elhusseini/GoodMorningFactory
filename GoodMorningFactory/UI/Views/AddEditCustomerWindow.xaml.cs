using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// نافذة إضافة وتعديل عميل.
    /// تم تجريدها من المنطق، وتعتمد الآن بشكل كامل على AddEditCustomerViewModel.
    /// </summary>
    public partial class AddEditCustomerWindow : Window
    {
        /// <summary>
        /// المنشئ الخاص بالنافذة.
        /// </summary>
        /// <param name="customer">كائن العميل المراد تعديله. إذا كان null، سيتم فتح النافذة في وضع الإضافة.</param>
        public AddEditCustomerWindow(Customer customer = null)
        {
            InitializeComponent();

            // إنشاء خدمة العميل
            // ملاحظة: في بنية متقدمة، يفضل حقن (Inject) الخدمة بدلاً من إنشائها هنا.
            ICustomerService customerService = new CustomerService();

            // إنشاء وتعيين ViewModel
            var viewModel = new AddEditCustomerViewModel(customerService, customer);

            // تعيين DataContext الخاص بالنافذة إلى الـ ViewModel
            // هذا يسمح بربط عناصر الواجهة (XAML) بخصائص وأوامر الـ ViewModel
            this.DataContext = viewModel;
        }
    }
}
