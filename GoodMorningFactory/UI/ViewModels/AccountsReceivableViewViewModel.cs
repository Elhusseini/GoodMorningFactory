using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AccountsReceivableViewViewModel : BaseViewModel
    {
        private readonly IAccountsReceivableService _arService;

        private ObservableCollection<AccountsReceivableViewModel> _agingReportItems;
        public ObservableCollection<AccountsReceivableViewModel> AgingReportItems
        {
            get => _agingReportItems;
            set { _agingReportItems = value; OnPropertyChanged(); }
        }

        public ICommand RecordPaymentCommand { get; }
        public ICommand SendReminderCommand { get; }
        public ICommand RefreshCommand { get; }

        public AccountsReceivableViewViewModel()
        {
            _arService = new AccountsReceivableService();
            AgingReportItems = new ObservableCollection<AccountsReceivableViewModel>();

            RecordPaymentCommand = new RelayCommand(ExecuteRecordPayment, CanExecuteAction);
            SendReminderCommand = new RelayCommand(ExecuteSendReminder, CanExecuteAction);
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var items = await _arService.GetAgingReportAsync();
                AgingReportItems = new ObservableCollection<AccountsReceivableViewModel>(items);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل تقرير الذمم المدينة: {ex.Message}", "خطأ");
            }
        }

        private void ExecuteRecordPayment(object parameter)
        {
            if (parameter is AccountsReceivableViewModel invoice)
            {
                var paymentWindow = new RecordSalePaymentWindow(invoice.SaleId);
                if (paymentWindow.ShowDialog() == true)
                {
                    LoadDataAsync();
                }
            }
        }

        private void ExecuteSendReminder(object parameter)
        {
            if (parameter is AccountsReceivableViewModel invoice)
            {
                MessageBox.Show($"تم إرسال تذكير للعميل '{invoice.CustomerName}' بخصوص الفاتورة رقم '{invoice.InvoiceNumber}'.", "إرسال تذكير", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanExecuteAction(object parameter)
        {
            return parameter is AccountsReceivableViewModel;
        }
    }
}