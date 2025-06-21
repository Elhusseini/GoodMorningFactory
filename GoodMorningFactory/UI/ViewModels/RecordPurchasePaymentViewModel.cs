using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class RecordPurchasePaymentViewModel : BaseViewModel
    {
        private readonly IAccountsPayableService _apService;
        private readonly int _purchaseId;
        private decimal _balanceDueValue;

        #region Properties
        public string InvoiceNumber { get; set; }
        public string TotalAmountText { get; set; }
        public string PreviouslyPaidText { get; set; }
        public string BalanceDueText { get; set; }
        public decimal AmountToPay { get; set; }
        #endregion

        public ICommand ConfirmPaymentCommand { get; }

        // مُنشئ لوضع التصميم
        public RecordPurchasePaymentViewModel() { }

        public RecordPurchasePaymentViewModel(int purchaseId)
        {
            _apService = new AccountsPayableService();
            _purchaseId = purchaseId;
            ConfirmPaymentCommand = new RelayCommand(ConfirmPayment, CanConfirmPayment);
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            var details = await _apService.GetPaymentDetailsAsync(_purchaseId);
            if (details == null) return;

            string currencySymbol = AppSettings.DefaultCurrencySymbol;
            InvoiceNumber = details.InvoiceNumber;
            TotalAmountText = $"{details.TotalAmount:N2} {currencySymbol}";
            PreviouslyPaidText = $"{details.PreviouslyPaid:N2} {currencySymbol}";
            _balanceDueValue = details.BalanceDue;
            BalanceDueText = $"{_balanceDueValue:N2} {currencySymbol}";
            AmountToPay = _balanceDueValue;

            OnPropertyChanged(string.Empty);
        }

        private bool CanConfirmPayment(object parameter)
        {
            return AmountToPay > 0 && AmountToPay <= _balanceDueValue;
        }

        private async void ConfirmPayment(object parameter)
        {
            try
            {
                await _apService.RecordPaymentAsync(_purchaseId, AmountToPay);
                MessageBox.Show("تم تسجيل الدفعة بنجاح.", "نجاح");
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
            }
        }
    }
}