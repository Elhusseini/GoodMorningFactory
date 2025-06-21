using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class CustomerStatementViewModel : BaseViewModel
    {
        private readonly ICustomerService _customerService;
        private readonly IPrintingService _printingService; // *** إضافة خدمة الطباعة ***
        private readonly int _customerId;

        #region الخصائص العامة
        private string _customerName;
        public string CustomerName
        {
            get => _customerName;
            set { _customerName = value; OnPropertyChanged(); }
        }

        private ObservableCollection<CustomerStatementItemViewModel> _statementItems;
        public ObservableCollection<CustomerStatementItemViewModel> StatementItems
        {
            get => _statementItems;
            set { _statementItems = value; OnPropertyChanged(); }
        }

        // *** بداية الإضافة: خصائص الملخص ***
        private decimal _totalDebit;
        public decimal TotalDebit
        {
            get => _totalDebit;
            set { _totalDebit = value; OnPropertyChanged(); }
        }

        private decimal _totalCredit;
        public decimal TotalCredit
        {
            get => _totalCredit;
            set { _totalCredit = value; OnPropertyChanged(); }
        }

        private decimal _finalBalance;
        public decimal FinalBalance
        {
            get => _finalBalance;
            set { _finalBalance = value; OnPropertyChanged(); }
        }
        // *** نهاية الإضافة ***
        #endregion

        #region الأوامر
        public RelayCommand PrintCommand { get; }
        public RelayCommand CloseCommand { get; }
        #endregion

        public CustomerStatementViewModel(ICustomerService customerService, IPrintingService printingService, int customerId)
        {
            _customerService = customerService;
            _printingService = printingService; // *** تهيئة خدمة الطباعة ***
            _customerId = customerId;

            // *** تهيئة الأوامر ***
            // *** بداية الإصلاح: استخدام تعبير lambda لاستدعاء الدالة غير المتزامنة ***
            PrintCommand = new RelayCommand(async _ => await PrintStatement(), _ => StatementItems != null && StatementItems.Any());
            // *** نهاية الإصلاح ***

            CloseCommand = new RelayCommand(CloseWindow);

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // جلب اسم العميل
                var customer = await _customerService.GetCustomerByIdAsync(_customerId);
                if (customer != null)
                {
                    CustomerName = $"كشف حساب العميل: {customer.CustomerName}";
                }

                // جلب حركات كشف الحساب
                var items = await _customerService.GetCustomerStatementAsync(_customerId);
                StatementItems = new ObservableCollection<CustomerStatementItemViewModel>(items);

                // *** بداية الإضافة: حساب الملخص ***
                CalculateSummary();
                // *** نهاية الإضافة ***
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل كشف الحساب: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // *** بداية الإضافة: دوال جديدة ***
        private void CalculateSummary()
        {
            if (StatementItems == null || !StatementItems.Any())
            {
                TotalDebit = 0;
                TotalCredit = 0;
                FinalBalance = 0;
                return;
            }

            TotalDebit = StatementItems.Sum(item => item.Debit);
            TotalCredit = StatementItems.Sum(item => item.Credit);
            FinalBalance = StatementItems.Last().Balance;
        }

        // *** بداية التعديل ***
        private async Task PrintStatement()
        {
            await _printingService.PrintCustomerStatementAsync(_customerId);
        }
        // *** نهاية التعديل ***

        private void CloseWindow(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
        // *** نهاية الإضافة ***
    }
}
