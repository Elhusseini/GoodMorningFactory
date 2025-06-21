// GoodMorningFactory/UI/ViewModels/AddEditAccountViewModel.cs
// *** الكود الكامل والمعدل - تمت إضافة خاصية IsBank ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditAccountViewModel : BaseViewModel
    {
        private readonly IChartOfAccountsService _chartOfAccountsService;
        private readonly int? _accountId;
        private Account _account;

        #region Properties
        private string _windowTitle;
        public string WindowTitle { get => _windowTitle; set { _windowTitle = value; OnPropertyChanged(); } }

        private string _accountNumber;
        public string AccountNumber { get => _accountNumber; set { _accountNumber = value; OnPropertyChanged(); } }

        private string _accountName;
        public string AccountName { get => _accountName; set { _accountName = value; OnPropertyChanged(); } }

        public IEnumerable<AccountType> AccountTypes => Enum.GetValues(typeof(AccountType)).Cast<AccountType>();
        private AccountType _selectedAccountType;
        public AccountType SelectedAccountType { get => _selectedAccountType; set { _selectedAccountType = value; OnPropertyChanged(); } }

        public ObservableCollection<Account> ParentAccounts { get; set; }
        private Account _selectedParentAccount;
        public Account SelectedParentAccount { get => _selectedParentAccount; set { _selectedParentAccount = value; OnPropertyChanged(); } }

        private bool _isActive;
        public bool IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged(); } }

        // ======================= بداية الإصلاح =======================
        private bool _isBank;
        public bool IsBank { get => _isBank; set { _isBank = value; OnPropertyChanged(); } }
        // ======================== نهاية الإصلاح ========================
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        #endregion

        public AddEditAccountViewModel(int? accountId = null)
        {
            _chartOfAccountsService = new ChartOfAccountsService();
            _accountId = accountId;
            ParentAccounts = new ObservableCollection<Account>();
            SaveCommand = new RelayCommand(async (p) => await Save(p), CanSave);

            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            try
            {
                var dto = await _chartOfAccountsService.GetDataForAddEditAccountAsync(_accountId);
                _account = dto.Account;

                ParentAccounts.Clear();
                ParentAccounts.Add(new Account { Id = 0, AccountName = "(بدون)" });
                dto.AllAccounts.ForEach(a => ParentAccounts.Add(a));

                AccountNumber = _account.AccountNumber;
                AccountName = _account.AccountName;
                SelectedAccountType = _account.AccountType;
                SelectedParentAccount = ParentAccounts.FirstOrDefault(p => p.Id == (_account.ParentAccountId ?? 0));
                IsActive = _account.IsActive;
                IsBank = _account.IsBank; // تحميل القيمة الحالية

                WindowTitle = _account.Id == 0 ? "إضافة حساب جديد" : "تعديل حساب";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(AccountNumber) && !string.IsNullOrWhiteSpace(AccountName);
        }

        private async Task Save(object parameter)
        {
            _account.AccountNumber = AccountNumber;
            _account.AccountName = AccountName;
            _account.AccountType = SelectedAccountType;
            _account.ParentAccountId = (SelectedParentAccount?.Id == 0) ? null : SelectedParentAccount?.Id;
            _account.IsActive = IsActive;
            _account.IsBank = IsBank; // حفظ القيمة الجديدة

            try
            {
                await _chartOfAccountsService.SaveAccountAsync(_account);
                MessageBox.Show("تم حفظ الحساب بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الحساب: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}