using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class RecordPaymentViewModel : BaseViewModel
    {
        private readonly ISalesService _salesService;
        private readonly int _saleId;
        private decimal _balanceDueValue;

        public string InvoiceNumber { get; set; }
        public string TotalAmountText { get; set; }
        public string PreviouslyPaidText { get; set; }
        public string BalanceDueText { get; set; }
        public decimal AmountToPay { get; set; }

        public RelayCommand ConfirmPaymentCommand { get; }

        // مُنشئ لوضع التصميم
        public RecordPaymentViewModel() { }

        public RecordPaymentViewModel(ISalesService salesService, int saleId)
        {
            _salesService = salesService;
            _saleId = saleId;
            ConfirmPaymentCommand = new RelayCommand(async (param) => await ConfirmPayment(param), _ => CanConfirmPayment());
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            var details = await _salesService.GetPaymentDetailsAsync(_saleId);
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

        private bool CanConfirmPayment()
        {
            return AmountToPay > 0 && AmountToPay <= _balanceDueValue;
        }

        private async Task ConfirmPayment(object parameter)
        {
            try
            {
                await _salesService.RecordPaymentAsync(_saleId, AmountToPay);
                MessageBox.Show("تم تسجيل الدفعة بنجاح.", "نجاح");
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
            }
        }
    }
}