// GoodMorningFactory/UI/ViewModels/AccountingPeriodsViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using Microsoft.EntityFrameworkCore; // <-- تأكد من وجود هذا السطر
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    // هذا الـ ViewModel الصغير يمثل الفترة المحاسبية في الواجهة
    public class AccountingPeriodViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedDate { get; set; }

        private bool _canClose;
        public bool CanClose { get => _canClose; set { _canClose = value; OnPropertyChanged(); } }
    }

    public class AccountingPeriodsViewModel : BaseViewModel
    {
        private ObservableCollection<AccountingPeriodViewModel> _periods;
        public ObservableCollection<AccountingPeriodViewModel> Periods
        {
            get => _periods;
            set { _periods = value; OnPropertyChanged(); }
        }

        public RelayCommand LoadPeriodsCommand { get; }
        public RelayCommand ClosePeriodCommand { get; }

        public AccountingPeriodsViewModel()
        {
            Periods = new ObservableCollection<AccountingPeriodViewModel>();
            LoadPeriodsCommand = new RelayCommand(async _ => await LoadPeriodsAsync());
            ClosePeriodCommand = new RelayCommand(async (param) => await ClosePeriodAsync(param));

            // تحميل البيانات عند إنشاء الـ ViewModel
            LoadPeriodsAsync();
        }

        private async Task LoadPeriodsAsync()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // نفس منطق الإنشاء التلقائي للفترات
                    var lastPeriod = await db.AccountingPeriods.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).FirstOrDefaultAsync();
                    int startYear = lastPeriod?.Year ?? DateTime.Now.Year;
                    int startMonth = lastPeriod?.Month + 1 ?? 1;
                    if (startMonth > 12) { startMonth = 1; startYear++; }

                    for (var dt = new DateTime(startYear, startMonth, 1); dt <= DateTime.Now; dt = dt.AddMonths(1))
                    {
                        if (!await db.AccountingPeriods.AnyAsync(p => p.Year == dt.Year && p.Month == dt.Month))
                        {
                            db.AccountingPeriods.Add(new AccountingPeriod { Year = dt.Year, Month = dt.Month, Status = PeriodStatus.Open });
                        }
                    }
                    await db.SaveChangesAsync();

                    // عرض الفترات
                    var dbPeriods = await db.AccountingPeriods
                                            .OrderByDescending(p => p.Year)
                                            .ThenByDescending(p => p.Month)
                                            .ToListAsync();

                    var viewModels = dbPeriods.Select(p => new AccountingPeriodViewModel
                    {
                        Id = p.Id,
                        Year = p.Year,
                        Month = p.Month,
                        Status = p.Status == PeriodStatus.Open ? "مفتوحة" : "مغلقة",
                        ClosedDate = p.ClosedDate,
                        CanClose = p.Status == PeriodStatus.Open && PermissionsService.CanAccess("Financials.Periods.Close")
                    }).ToList();

                    Periods = new ObservableCollection<AccountingPeriodViewModel>(viewModels);
                    OnPropertyChanged(nameof(Periods));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الفترات المحاسبية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ClosePeriodAsync(object parameter)
        {
            if (parameter is AccountingPeriodViewModel periodVM)
            {
                var result = MessageBox.Show($"هل أنت متأكد من إغلاق الفترة {periodVM.Month}-{periodVM.Year}؟\nلن تتمكن من تسجيل أو تعديل أي حركات في هذه الفترة بعد إغلاقها.", "تأكيد الإغلاق", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            var period = await db.AccountingPeriods.FindAsync(periodVM.Id);
                            if (period != null)
                            {
                                period.Status = PeriodStatus.Closed;
                                period.ClosedDate = DateTime.Now;
                                period.ClosedByUserId = CurrentUserService.LoggedInUser?.Id;
                                await db.SaveChangesAsync(true); // *** بداية الإصلاح: إضافة true ***
                                await LoadPeriodsAsync();
                                MessageBox.Show("تم إغلاق الفترة بنجاح.", "نجاح");
                            }
                        }
                    }
                    catch (Exception ex) { MessageBox.Show($"فشل إغلاق الفترة: {ex.Message}", "خطأ"); }
                }
            }
        }
    }
}