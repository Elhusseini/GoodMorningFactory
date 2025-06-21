// GoodMorningFactory/UI/ViewModels/AddEditCustomerViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel لنافذة إضافة وتعديل عميل.
    /// يدير منطق التحقق من الصحة، والحفظ، والإلغاء.
    /// </summary>
    public class AddEditCustomerViewModel : BaseViewModel
    {
        #region الخدمات
        private readonly ICustomerService _customerService;
        #endregion

        #region الخصائص
        private Customer _currentCustomer;
        public Customer CurrentCustomer { get => _currentCustomer; set { _currentCustomer = value; OnPropertyChanged(); } }

        private string _windowTitle;
        public string WindowTitle { get => _windowTitle; set { _windowTitle = value; OnPropertyChanged(); } }

        public bool IsEditMode { get; private set; }
        #endregion

        #region الأوامر
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        #endregion

        public AddEditCustomerViewModel(ICustomerService customerService, Customer customerToEdit = null)
        {
            _customerService = customerService;

            // استدعاء دالة التهيئة غير المتزامنة التي ستقوم بكل العمل
            InitializeAsync(customerToEdit);

            SaveCommand = new RelayCommand(async (param) => await Save(param), CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        /// <summary>
        /// تقوم هذه الدالة بتهيئة الـ ViewModel.
        /// إذا كان في وضع الإضافة، ستقوم باستدعاء الخدمة لجلب كود العميل الجديد.
        /// </summary>
        private async void InitializeAsync(Customer customerToEdit)
        {
            if (customerToEdit == null) // وضع الإضافة
            {
                IsEditMode = false;
                WindowTitle = "إضافة عميل جديد";
                CurrentCustomer = new Customer
                {
                    // --- بداية التعديل: استدعاء الخدمة لجلب الكود الجديد ---
                    CustomerCode = await _customerService.GetNextCustomerCodeAsync(),
                    // --- نهاية التعديل ---
                    IsActive = true
                };
            }
            else // وضع التعديل
            {
                IsEditMode = true;
                WindowTitle = $"تعديل بيانات العميل: {customerToEdit.CustomerName}";
                // استخدام نسخة من الكائن لتجنب التعديل المباشر على القائمة الرئيسية قبل الحفظ
                CurrentCustomer = new Customer
                {
                    Id = customerToEdit.Id,
                    CustomerCode = customerToEdit.CustomerCode,
                    CustomerName = customerToEdit.CustomerName,
                    ContactPerson = customerToEdit.ContactPerson,
                    Email = customerToEdit.Email,
                    PhoneNumber = customerToEdit.PhoneNumber,
                    TaxNumber = customerToEdit.TaxNumber,
                    BillingAddress = customerToEdit.BillingAddress,
                    ShippingAddress = customerToEdit.ShippingAddress,
                    DefaultPaymentTerms = customerToEdit.DefaultPaymentTerms,
                    CreditLimit = customerToEdit.CreditLimit,
                    IsActive = customerToEdit.IsActive
                };
            }
            // إعلام الواجهة بأن كل الخصائص قد تغيرت
            OnPropertyChanged(nameof(CurrentCustomer));
            OnPropertyChanged(nameof(WindowTitle));
        }

        private bool CanSave(object parameter)
        {
            if (CurrentCustomer == null) return false;
            return !string.IsNullOrWhiteSpace(CurrentCustomer.CustomerName) &&
                   !string.IsNullOrWhiteSpace(CurrentCustomer.CustomerCode);
        }

        private async Task Save(object parameter)
        {
            if (!(parameter is Window window)) return;

            try
            {
                if (IsEditMode)
                {
                    await _customerService.UpdateCustomerAsync(CurrentCustomer);
                }
                else
                {
                    await _customerService.AddCustomerAsync(CurrentCustomer);
                }

                // إذا تم الحفظ بنجاح، أغلق النافذة مع نتيجة إيجابية
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حفظ بيانات العميل: {ex.Message}", "خطأ في الحفظ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel(object parameter)
        {
            if (parameter is Window window)
            {
                window.DialogResult = false;
            }
        }
    }
}