// UI/ViewModels/ProcessPayrollViewModel.cs
// *** ملف جديد: ViewModel لنافذة معالجة مسير الرواتب ***

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ProcessPayrollViewModel : BaseViewModel
    {
        private readonly IHRService _hrService;

        #region الخصائص (Properties)
        // قوائم الفلاتر (الأشهر والسنوات)
        public List<int> Months { get; } = Enumerable.Range(1, 12).ToList();
        public List<int> Years { get; } = Enumerable.Range(DateTime.Now.Year - 5, 10).ToList();

        // الشهر والسنة المختاران
        private int _selectedMonth = DateTime.Now.Month;
        public int SelectedMonth { get => _selectedMonth; set { _selectedMonth = value; OnPropertyChanged(); } }

        private int _selectedYear = DateTime.Now.Year;
        public int SelectedYear { get => _selectedYear; set { _selectedYear = value; OnPropertyChanged(); } }

        // قائمة تفاصيل الرواتب المحسوبة للعرض في الجدول
        public ObservableCollection<PayslipViewModel> Payslips { get; set; } = new ObservableCollection<PayslipViewModel>();

        private bool _isProcessing;
        public bool IsProcessing { get => _isProcessing; set { _isProcessing = value; OnPropertyChanged(); } }
        #endregion

        #region الأوامر (Commands)
        public ICommand CalculatePayrollCommand { get; }
        public ICommand ConfirmPayrollCommand { get; }
        #endregion

        public ProcessPayrollViewModel(IHRService hrService)
        {
            _hrService = hrService;

            // ربط الأوامر بالدوال
            CalculatePayrollCommand = new AsyncRelayCommand(CalculatePayrollAsync);
            // شرط تفعيل زر التأكيد هو وجود بيانات محسوبة في الجدول
            ConfirmPayrollCommand = new AsyncRelayCommand(ConfirmPayrollAsync, () => Payslips.Any());
        }

        /// <summary>
        /// دالة تقوم باستدعاء الخدمة لحساب الرواتب وعرضها في الجدول.
        /// </summary>
        private async Task CalculatePayrollAsync()
        {
            IsProcessing = true;
            try
            {
                var calculatedPayslips = await _hrService.CalculatePayslipsAsync(SelectedYear, SelectedMonth);
                Payslips.Clear();
                foreach (var slip in calculatedPayslips)
                {
                    Payslips.Add(slip);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ في الاحتساب", MessageBoxButton.OK, MessageBoxImage.Error);
                Payslips.Clear(); // مسح البيانات في حالة وجود خطأ (مثل مسير موجود مسبقاً)
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// دالة تقوم بحفظ واعتماد مسير الرواتب النهائي.
        /// </summary>
        private async Task ConfirmPayrollAsync()
        {
            IsProcessing = true;
            try
            {
                var payroll = new Payroll
                {
                    Year = SelectedYear,
                    Month = SelectedMonth,
                    DateProcessed = DateTime.Today,
                    TotalAmount = Payslips.Sum(p => p.NetSalary)
                };

                await _hrService.ProcessAndSavePayrollAsync(payroll, Payslips.ToList());
                MessageBox.Show("تم حفظ واعتماد مسير الرواتب والقيد المحاسبي بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                Payslips.Clear(); // مسح البيانات بعد الحفظ الناجح
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}