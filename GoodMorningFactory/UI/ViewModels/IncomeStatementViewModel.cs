// GoodMorningFactory/UI/ViewModels/IncomeStatementViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class IncomeStatementViewModel : BaseViewModel
    {
        private readonly IFinancialReportsService _financialReportsService;

        #region Properties
        private DateTime _fromDate;
        public DateTime FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); } }

        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }

        private string _reportDateRange;
        public string ReportDateRange { get => _reportDateRange; set { _reportDateRange = value; OnPropertyChanged(); } }

        public ObservableCollection<IncomeStatementItemViewModel> Revenues { get; set; }
        public ObservableCollection<IncomeStatementItemViewModel> Expenses { get; set; }

        private decimal _totalRevenue;
        public decimal TotalRevenue { get => _totalRevenue; set { _totalRevenue = value; OnPropertyChanged(); } }

        private decimal _totalExpenses;
        public decimal TotalExpenses { get => _totalExpenses; set { _totalExpenses = value; OnPropertyChanged(); } }

        private decimal _netIncome;
        public decimal NetIncome { get => _netIncome; set { _netIncome = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public ICommand GenerateReportCommand { get; }
        #endregion

        public IncomeStatementViewModel()
        {
            _financialReportsService = new FinancialReportsService();

            // إعداد التواريخ الافتراضية (بداية الشهر الحالي حتى اليوم)
            var today = DateTime.Today;
            FromDate = new DateTime(today.Year, today.Month, 1);
            ToDate = today;

            Revenues = new ObservableCollection<IncomeStatementItemViewModel>();
            Expenses = new ObservableCollection<IncomeStatementItemViewModel>();

            GenerateReportCommand = new RelayCommand(async _ => await GenerateReportAsync());

            // تحميل التقرير عند فتح الشاشة
            GenerateReportAsync();
        }

        private async Task GenerateReportAsync()
        {
            // جلب البيانات من الخدمة (الخدمة ترجع ViewModel جاهز)
            var resultViewModel = await _financialReportsService.GetIncomeStatementAsync(FromDate, ToDate.Date.AddDays(1).AddTicks(-1));

            // تحديث الخصائص في الـ ViewModel الحالي
            ReportDateRange = resultViewModel.ReportDateRange;

            Revenues.Clear();
            resultViewModel.Revenues.ToList().ForEach(r => Revenues.Add(r));

            Expenses.Clear();
            resultViewModel.Expenses.ToList().ForEach(e => Expenses.Add(e));

            // تحديث الإجماليات
            CalculateTotals();
        }

        /// <summary>
        /// دالة داخلية لحساب الإجماليات
        /// </summary>
        public void CalculateTotals()
        {
            TotalRevenue = Revenues.Sum(r => r.Balance);
            TotalExpenses = Expenses.Sum(e => e.Balance);
            NetIncome = TotalRevenue - TotalExpenses; // لاحظ: المصاريف ستكون سالبة بطبيعتها
        }
    }
}