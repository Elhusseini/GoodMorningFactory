// GoodMorningFactory/UI/ViewModels/TrialBalanceViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    // تم حذف تعريف TrialBalanceItemViewModel من هنا لأنه موجود في ملفه الخاص

    // هذا هو الـ ViewModel الرئيسي للواجهة
    public class TrialBalanceViewModel : BaseViewModel
    {
        private readonly IFinancialReportsService _financialReportsService;

        #region Properties
        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }

        private ObservableCollection<TrialBalanceItemViewModel> _reportItems;
        public ObservableCollection<TrialBalanceItemViewModel> ReportItems { get => _reportItems; set { _reportItems = value; OnPropertyChanged(); } }

        private decimal _totalDebit;
        public decimal TotalDebit { get => _totalDebit; set { _totalDebit = value; OnPropertyChanged(); } }

        private decimal _totalCredit;
        public decimal TotalCredit { get => _totalCredit; set { _totalCredit = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public ICommand GenerateReportCommand { get; }
        #endregion

        public TrialBalanceViewModel()
        {
            _financialReportsService = new FinancialReportsService();
            ReportItems = new ObservableCollection<TrialBalanceItemViewModel>();
            ToDate = DateTime.Today; // تعيين تاريخ اليوم كقيمة افتراضية

            GenerateReportCommand = new RelayCommand(async _ => await GenerateReportAsync());

            // تحميل التقرير تلقائياً عند فتح الشاشة لأول مرة
            GenerateReportAsync();
        }

        private async Task GenerateReportAsync()
        {
            try
            {
                // إضافة يوم واحد وطرح تكة واحدة للحصول على نهاية اليوم (23:59:59)
                var endDate = ToDate.Date.AddDays(1).AddTicks(-1);
                var items = await _financialReportsService.GetTrialBalanceAsync(endDate);

                ReportItems.Clear();
                foreach (var item in items)
                {
                    ReportItems.Add(item);
                }

                // حساب الإجماليات
                TotalDebit = ReportItems.Sum(i => i.DebitBalance);
                TotalCredit = ReportItems.Sum(i => i.CreditBalance);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء إنشاء التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}