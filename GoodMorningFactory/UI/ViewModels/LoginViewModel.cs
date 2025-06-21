using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class LoginViewModel : BaseViewModel // يرث من نفس الـ BaseViewModel المستخدم في مشروعك
    {
        private readonly IAuthenticationService _authenticationService;
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isLoggingIn;

        // حدث لإعلام الواجهة بنجاح تسجيل الدخول
        public event Action LoginSuccess;

        // خاصية لاسم المستخدم مع التعديل
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username)); // <-- تم التصحيح هنا
            }
        }

        // خاصية لكلمة المرور مع التعديل
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password)); // <-- تم التصحيح هنا
            }
        }

        // خاصية لرسالة الخطأ التي ستظهر للمستخدم مع التعديل
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage)); // <-- تم التصحيح هنا
            }
        }

        // خاصية للتحكم في حالة الزر مع التعديل
        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set
            {
                _isLoggingIn = value;
                OnPropertyChanged(nameof(IsLoggingIn)); // <-- تم التصحيح هنا
            }
        }

        // الأمر الذي سيتم ربطه بزر "دخول"
        public ICommand LoginCommand { get; }

        public LoginViewModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync, CanExecuteLogin);
        }

        // دالة تحدد ما إذا كان يمكن تنفيذ أمر الدخول
        private bool CanExecuteLogin()
        {
            // نتحقق من اسم المستخدم وكلمة المرور من الخصائص مباشرة
            return !IsLoggingIn && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrEmpty(Password);
        }

        // الدالة التي يتم تنفيذها عند الضغط على زر الدخول
        private async Task ExecuteLoginAsync()
        {
            IsLoggingIn = true;
            ErrorMessage = string.Empty;
            ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();

            try
            {
                var user = await _authenticationService.LoginAsync(Username, Password);

                if (user != null)
                {
                    LoginSuccess?.Invoke();
                }
                else
                {
                    ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة, أو أن الحساب غير نشط.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"حدث خطأ: {ex.Message}";
            }
            finally
            {
                IsLoggingIn = false;
                ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }
    }
}