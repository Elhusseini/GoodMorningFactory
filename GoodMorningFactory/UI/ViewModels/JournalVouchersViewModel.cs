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
    public class JournalVouchersViewModel : BaseViewModel
    {
        private readonly IJournalVoucherService _journalVoucherService;

        private ObservableCollection<JournalVoucherViewModel> _vouchers;
        public ObservableCollection<JournalVoucherViewModel> Vouchers
        {
            get => _vouchers;
            set { _vouchers = value; OnPropertyChanged(); }
        }

        public ICommand AddVoucherCommand { get; }
        public ICommand ReverseVoucherCommand { get; }
        public ICommand RefreshCommand { get; }

        public JournalVouchersViewModel()
        {
            _journalVoucherService = new JournalVoucherService();

            AddVoucherCommand = new RelayCommand(AddVoucher);
            ReverseVoucherCommand = new RelayCommand(ReverseVoucher, CanReverseVoucher);
            RefreshCommand = new RelayCommand(async _ => await LoadVouchersAsync());

            LoadVouchersAsync();
        }

        private async Task LoadVouchersAsync()
        {
            try
            {
                var vouchersList = await _journalVoucherService.GetVouchersAsync();
                Vouchers = new ObservableCollection<JournalVoucherViewModel>(vouchersList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل القيود: {ex.Message}", "خطأ");
            }
        }

        private void AddVoucher(object parameter)
        {
            var addWindow = new AddEditJournalVoucherView();
            if (addWindow.ShowDialog() == true)
            {
                LoadVouchersAsync();
            }
        }

        private async void ReverseVoucher(object parameter)
        {
            if (parameter is JournalVoucherViewModel voucherToReverse)
            {
                var result = MessageBox.Show($"هل أنت متأكد من عكس القيد رقم '{voucherToReverse.VoucherNumber}'؟\nسيتم إنشاء قيد جديد يعكس هذا القيد.", "تأكيد عكس القيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;

                try
                {
                    await _journalVoucherService.ReverseVoucherAsync(voucherToReverse.Id);
                    await LoadVouchersAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشلت عملية عكس القيد: {ex.Message}", "خطأ");
                }
            }
        }

        private bool CanReverseVoucher(object parameter)
        {
            // يمكن عكس القيد فقط إذا كان مرحّلاً (Posted)
            return parameter is JournalVoucherViewModel voucher && voucher.Status == Data.Models.VoucherStatus.Posted;
        }
    }
}
