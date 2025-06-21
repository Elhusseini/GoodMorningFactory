// UI/Views/AccountingPeriodsView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة الفترات المحاسبية ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Core.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public class AccountingPeriodViewModel
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedDate { get; set; }
        public bool CanClose { get; set; }
    }

    public partial class AccountingPeriodsView : UserControl
    {
        public AccountingPeriodsView()
        {
            InitializeComponent();
            LoadPeriods();
        }

        private void LoadPeriods()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // التأكد من وجود فترة لكل شهر حتى الشهر الحالي
                    var lastPeriod = db.AccountingPeriods.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).FirstOrDefault();
                    int startYear = lastPeriod?.Year ?? DateTime.Now.Year;
                    int startMonth = lastPeriod?.Month + 1 ?? 1;
                    if (startMonth > 12) { startMonth = 1; startYear++; }

                    for (var dt = new DateTime(startYear, startMonth, 1); dt <= DateTime.Now; dt = dt.AddMonths(1))
                    {
                        if (!db.AccountingPeriods.Any(p => p.Year == dt.Year && p.Month == dt.Month))
                        {
                            db.AccountingPeriods.Add(new AccountingPeriod { Year = dt.Year, Month = dt.Month, Status = PeriodStatus.Open });
                        }
                    }
                    db.SaveChanges();

                    // عرض الفترات
                    var periods = db.AccountingPeriods
                                    .OrderByDescending(p => p.Year)
                                    .ThenByDescending(p => p.Month)
                                    .ToList();

                    var viewModels = periods.Select(p => new AccountingPeriodViewModel
                    {
                        Id = p.Id,
                        Year = p.Year,
                        Month = p.Month,
                        Status = p.Status == PeriodStatus.Open ? "مفتوحة" : "مغلقة",
                        ClosedDate = p.ClosedDate,
                        CanClose = p.Status == PeriodStatus.Open && PermissionsService.CanAccess("Financials.Periods.Close")
                    }).ToList();

                    PeriodsDataGrid.ItemsSource = viewModels;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الفترات المحاسبية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClosePeriodButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is AccountingPeriodViewModel periodVM)
            {
                var result = MessageBox.Show($"هل أنت متأكد من إغلاق الفترة {periodVM.Month}-{periodVM.Year}؟\nلن تتمكن من تسجيل أو تعديل أي حركات في هذه الفترة بعد إغلاقها.", "تأكيد الإغلاق", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            var period = db.AccountingPeriods.Find(periodVM.Id);
                            if (period != null)
                            {
                                period.Status = PeriodStatus.Closed;
                                period.ClosedDate = DateTime.Now;
                                period.ClosedByUserId = CurrentUserService.LoggedInUser?.Id;
                                db.SaveChanges();
                                LoadPeriods();
                                MessageBox.Show("تم إغلاق الفترة بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل إغلاق الفترة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}