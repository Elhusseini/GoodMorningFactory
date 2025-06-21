using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ChartOfAccountsViewModel : BaseViewModel
    {
        private readonly IChartOfAccountsService _chartOfAccountsService;
        private bool _isInitialized = false;

        #region Properties
        private ObservableCollection<AccountViewModel> _accountsTree;
        public ObservableCollection<AccountViewModel> AccountsTree
        {
            get => _accountsTree;
            set { _accountsTree = value; OnPropertyChanged(); }
        }

        private AccountViewModel _selectedAccount;
        public AccountViewModel SelectedAccount
        {
            get => _selectedAccount;
            set { _selectedAccount = value; OnPropertyChanged(); LoadLedgerEntriesAsync(); }
        }

        private ObservableCollection<LedgerEntryViewModel> _ledgerEntries;
        public ObservableCollection<LedgerEntryViewModel> LedgerEntries
        {
            get => _ledgerEntries;
            set { _ledgerEntries = value; OnPropertyChanged(); }
        }

        private string _ledgerHeader = "دفتر الأستاذ العام";
        public string LedgerHeader
        {
            get => _ledgerHeader;
            set { _ledgerHeader = value; OnPropertyChanged(); }
        }

        public bool CanManageAccounts { get; }
        #endregion

        #region Commands
        public ICommand LoadDataCommand { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand EditAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand SelectedItemChangedCommand { get; }
        #endregion

        public ChartOfAccountsViewModel()
        {
            _chartOfAccountsService = new ChartOfAccountsService();
            CanManageAccounts = PermissionsService.CanAccess("Financials.ChartOfAccounts.Manage");

            LoadDataCommand = new RelayCommand(async _ => await InitializeAsync());
            AddAccountCommand = new RelayCommand(AddAccount, _ => CanManageAccounts);
            EditAccountCommand = new RelayCommand(EditAccount, CanEditOrDelete);
            DeleteAccountCommand = new RelayCommand(DeleteAccount, CanEditOrDelete);
            SelectedItemChangedCommand = new RelayCommand(OnSelectedItemChanged);
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;
            await LoadAccountTreeAsync();
            _isInitialized = true;
        }

        private async Task LoadAccountTreeAsync()
        {
            try
            {
                AccountsTree = await _chartOfAccountsService.GetAccountTreeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل شجرة الحسابات: {ex.Message}", "خطأ");
            }
        }

        private async void LoadLedgerEntriesAsync()
        {
            if (SelectedAccount == null)
            {
                LedgerEntries = new ObservableCollection<LedgerEntryViewModel>();
                LedgerHeader = "دفتر الأستاذ العام";
                return;
            }

            try
            {
                LedgerHeader = $"دفتر الأستاذ للحساب: {SelectedAccount.DisplayName}";
                var entries = await _chartOfAccountsService.GetLedgerEntriesAsync(SelectedAccount.Model.Id);
                LedgerEntries = new ObservableCollection<LedgerEntryViewModel>(entries);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل دفتر الأستاذ: {ex.Message}", "خطأ");
            }
        }

        private void OnSelectedItemChanged(object selectedItem)
        {
            SelectedAccount = selectedItem as AccountViewModel;
        }

        private void AddAccount(object parameter)
        {
            var addWindow = new AddEditAccountWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadAccountTreeAsync();
            }
        }

        private void EditAccount(object parameter)
        {
            var editWindow = new AddEditAccountWindow(SelectedAccount.Model.Id);
            if (editWindow.ShowDialog() == true)
            {
                LoadAccountTreeAsync();
            }
        }

        private async void DeleteAccount(object parameter)
        {
            var result = MessageBox.Show($"هل أنت متأكد من حذف الحساب: {SelectedAccount.DisplayName}؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _chartOfAccountsService.DeleteAccountAsync(SelectedAccount.Model.Id);
                SelectedAccount = null;
                await LoadAccountTreeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ في الحذف");
            }
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedAccount != null && CanManageAccounts;
        }
    }
}