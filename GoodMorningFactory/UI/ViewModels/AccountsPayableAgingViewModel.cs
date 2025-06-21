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
    public class AccountsPayableAgingViewModel : BaseViewModel
    {
        private readonly IAccountsPayableService _apService;

        private ObservableCollection<AccountsPayableViewModel> _agingReportItems;
        public ObservableCollection<AccountsPayableViewModel> AgingReportItems
        {
            get => _agingReportItems;
            set { _agingReportItems = value; OnPropertyChanged(); }
        }

        public ICommand RecordPaymentCommand { get; }
        public ICommand RefreshCommand { get; }

        public AccountsPayableAgingViewModel()
        {
            _apService = new AccountsPayableService();
            AgingReportItems = new ObservableCollection<AccountsPayableViewModel>();

            RecordPaymentCommand = new RelayCommand(ExecuteRecordPayment, CanExecuteAction);
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var items = await _apService.GetAgingReportAsync();
                AgingReportItems = new ObservableCollection<AccountsPayableViewModel>(items);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل تقرير الذمم الدائنة: {ex.Message}", "خطأ");
            }
        }

        private void ExecuteRecordPayment(object parameter)
        {
            if (parameter is AccountsPayableViewModel invoice)
            {
                var paymentWindow = new RecordPurchasePaymentWindow(invoice.PurchaseId);
                if (paymentWindow.ShowDialog() == true)
                {
                    LoadDataAsync();
                }
            }
        }

        private bool CanExecuteAction(object parameter) => parameter is AccountsPayableViewModel;
    }
}