using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // 1. إنشاء خدمة المصادقة
            var authService = new AuthenticationService();

            // 2. إنشاء الـ ViewModel وتمرير الخدمة إليه
            var viewModel = new LoginViewModel(authService);

            // 3. الاشتراك في حدث نجاح تسجيل الدخول
            viewModel.LoginSuccess += () =>
            {
                // عند نجاح الدخول، أغلق النافذة بنتيجة إيجابية
                this.DialogResult = true;
            };

            // 4. تعيين الـ ViewModel كـ DataContext للواجهة
            this.DataContext = viewModel;
        }
    }
}