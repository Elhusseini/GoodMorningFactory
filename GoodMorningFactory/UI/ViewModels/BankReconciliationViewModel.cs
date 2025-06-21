// GoodMorningFactory/UI/ViewModels/BankReconciliationViewModel.cs
// *** الكود الكامل والمصحح - تم إصلاح المعادلة الحسابية ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class BankReconciliationViewModel : BaseViewModel
    {
        private readonly IBankReconciliationService _reconService;

        #region Properties
        public ObservableCollection<Account> BankAccounts { get; } = new ObservableCollection<Account>();
        public ObservableCollection<ReconciliationTransactionViewModel> Transactions { get; } = new ObservableCollection<ReconciliationTransactionViewModel>();

        private Account _selectedBankAccount;
        public Account SelectedBankAccount { get => _selectedBankAccount; set { _selectedBankAccount = value; OnPropertyChanged(); LoadTransactionsAsync(); } }

        private DateTime _statementDate = DateTime.Now;
        public DateTime StatementDate { get => _statementDate; set { _statementDate = value; OnPropertyChanged(); LoadTransactionsAsync(); } }

        private decimal _statementBalance;
        public decimal StatementBalance { get => _statementBalance; set { _statementBalance = value; OnPropertyChanged(); UpdateSummary(); } }

        private decimal _bookBalance;
        public decimal BookBalance { get => _bookBalance; set { _bookBalance = value; OnPropertyChanged(); OnPropertyChanged(nameof(BookBalanceFormatted)); } }
        public string BookBalanceFormatted => $"{BookBalance:N2} {AppSettings.DefaultCurrencySymbol}";

        private decimal _clearedDebits;
        public decimal ClearedDebits { get => _clearedDebits; set { _clearedDebits = value; OnPropertyChanged(); OnPropertyChanged(nameof(ClearedDebitsFormatted)); } }
        public string ClearedDebitsFormatted => $"{ClearedDebits:N2} {AppSettings.DefaultCurrencySymbol}";

        private decimal _clearedCredits;
        public decimal ClearedCredits { get => _clearedCredits; set { _clearedCredits = value; OnPropertyChanged(); OnPropertyChanged(nameof(ClearedCreditsFormatted)); } }
        public string ClearedCreditsFormatted => $"{ClearedCredits:N2} {AppSettings.DefaultCurrencySymbol}";

        private decimal _reconciledBalance;
        public decimal ReconciledBalance { get => _reconciledBalance; set { _reconciledBalance = value; OnPropertyChanged(); OnPropertyChanged(nameof(ReconciledBalanceFormatted)); } }
        public string ReconciledBalanceFormatted => $"{ReconciledBalance:N2} {AppSettings.DefaultCurrencySymbol}";

        private decimal _difference;
        public decimal Difference { get => _difference; set { _difference = value; OnPropertyChanged(); OnPropertyChanged(nameof(DifferenceFormatted)); } }
        public string DifferenceFormatted => $"{Difference:N2} {AppSettings.DefaultCurrencySymbol}";

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        #endregion

        public ICommand SaveReconciliationCommand { get; }

        public BankReconciliationViewModel()
        {
            _reconService = new BankReconciliationService();
            SaveReconciliationCommand = new RelayCommand(async (p) => await SaveAsync(), (p) => Difference == 0 && Transactions.Any(t => t.IsSelected));
            LoadBankAccountsAsync();
        }

        private async void LoadBankAccountsAsync()
        {
            IsLoading = true;
            try
            {
                var accounts = await _reconService.GetBankAccountsAsync();
                BankAccounts.Clear();
                foreach (var acc in accounts) BankAccounts.Add(acc);
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل الحسابات البنكية: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async void LoadTransactionsAsync()
        {
            if (SelectedBankAccount == null) return;
            IsLoading = true;
            Transactions.Clear();
            try
            {
                BookBalance = await _reconService.GetBookBalanceAsync(SelectedBankAccount.Id, StatementDate);
                var transactions = await _reconService.GetUnreconciledTransactionsAsync(SelectedBankAccount.Id, StatementDate);
                foreach (var t in transactions)
                {
                    var vm = new ReconciliationTransactionViewModel(t);
                    vm.PropertyChanged += (s, e) => { if (e.PropertyName == "IsSelected") UpdateSummary(); };
                    Transactions.Add(vm);
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل الحركات: {ex.Message}", "خطأ"); }
            finally
            {
                IsLoading = false;
                UpdateSummary();
            }
        }

        private void UpdateSummary()
        {
            if (SelectedBankAccount == null) return;

            ClearedDebits = Transactions.Where(t => t.IsSelected).Sum(t => t.Debit);
            ClearedCredits = Transactions.Where(t => t.IsSelected).Sum(t => t.Credit);

            // ======================= بداية الإصلاح الرئيسي =======================
            // المعادلة الصحيحة للتسوية:
            // الرصيد الدفتري المعدل = رصيد الدفاتر - الحركات المدينة غير المسواة + الحركات الدائنة غير المسواة
            // (بافتراض أن المدين يزيد الرصيد والدائن ينقصه في دفتر الأستاذ)
            decimal outstandingDebits = Transactions.Where(t => !t.IsSelected).Sum(t => t.Debit);
            decimal outstandingCredits = Transactions.Where(t => !t.IsSelected).Sum(t => t.Credit);

            // تم تغيير اسم المتغير إلى ReconciledBalance ليعكس المعنى الصحيح
            ReconciledBalance = BookBalance - outstandingDebits + outstandingCredits;
            Difference = ReconciledBalance - StatementBalance;
            // ======================== نهاية الإصلاح الرئيسي ========================
        }

        private async Task SaveAsync()
        {
            IsLoading = true;
            try
            {
                var reconciliation = new BankReconciliation
                {
                    BankAccountId = SelectedBankAccount.Id,
                    StatementDate = StatementDate,
                    StatementEndingBalance = StatementBalance,
                    BookBalance = this.BookBalance
                };

                var reconciledIds = Transactions.Where(t => t.IsSelected).Select(t => t.JournalItemId).ToList();

                await _reconService.SaveReconciliationAsync(reconciliation, reconciledIds);
                MessageBox.Show("تم حفظ التسوية البنكية بنجاح.", "نجاح");
                LoadTransactionsAsync();
            }
            catch (Exception ex) { MessageBox.Show($"فشل حفظ التسوية: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }
    }

    // ViewModel المساعد لعرض حركة واحدة
    public class ReconciliationTransactionViewModel : BaseViewModel
    {
        public int JournalItemId { get; }
        public DateTime Date { get; }
        public string Description { get; }
        public decimal Debit { get; }
        public decimal Credit { get; }
        public string DebitFormatted => $"{Debit:N2} {AppSettings.DefaultCurrencySymbol}";
        public string CreditFormatted => $"{Credit:N2} {AppSettings.DefaultCurrencySymbol}";

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }

        public ReconciliationTransactionViewModel(JournalVoucherItem item)
        {
            JournalItemId = item.Id;
            Date = item.JournalVoucher.VoucherDate;
            Description = item.Description ?? item.JournalVoucher.Description;
            Debit = item.Debit;
            Credit = item.Credit;
        }
    }
}