// UI/Views/ProcessPayrollWindow.xaml.cs
// *** تحديث نهائي: دمج منطق حساب الغياب مع الكود الفعلي ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class ProcessPayrollWindow : Window
    {
        private List<PayslipViewModel> _payslips = new List<PayslipViewModel>();

        public ProcessPayrollWindow()
        {
            InitializeComponent();
            MonthComboBox.ItemsSource = Enumerable.Range(1, 12);
            YearComboBox.ItemsSource = Enumerable.Range(DateTime.Now.Year - 5, 10);
            MonthComboBox.SelectedItem = DateTime.Now.Month;
            YearComboBox.SelectedItem = DateTime.Now.Year;
        }

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            int year = (int)YearComboBox.SelectedItem;
            int month = (int)MonthComboBox.SelectedItem;

            using (var db = new DatabaseContext())
            {
                var employees = db.Employees.Where(emp => emp.Status == EmployeeStatus.Active).ToList();
                _payslips.Clear();

                var payrollStartDate = new DateTime(year, month, 1);
                var payrollEndDate = payrollStartDate.AddMonths(1).AddDays(-1);

                foreach (var emp in employees)
                {
                    // --- حساب الإجازات غير المدفوعة (من الكود الأصلي) ---
                    double unpaidLeaveDaysInMonth = 0;
                    var unpaidLeaveRequests = db.LeaveRequests
                        .Include(lr => lr.LeaveType)
                        .Where(lr => lr.EmployeeId == emp.Id &&
                                     lr.Status == LeaveRequestStatus.Approved &&
                                     lr.LeaveType.IsPaid == false &&
                                     lr.StartDate <= payrollEndDate &&
                                     lr.EndDate >= payrollStartDate)
                        .ToList();

                    foreach (var leave in unpaidLeaveRequests)
                    {
                        var leaveStart = leave.StartDate > payrollStartDate ? leave.StartDate : payrollStartDate;
                        var leaveEnd = leave.EndDate < payrollEndDate ? leave.EndDate : payrollEndDate;
                        unpaidLeaveDaysInMonth += (leaveEnd - leaveStart).TotalDays + 1;
                    }

                    // *** بداية الإضافة: منطق حساب أيام الغياب ***
                    double absenceDaysInMonth = 0;
                    // افترض أن لديك جدول اسمه "Attendance" لتسجيل الحضور والغياب
                    // ستحتاج إلى استعلام مشابه للتالي (قم بتعديله ليناسب هيكل بياناتك):
                    /*
                    var absenceRecords = db.AttendanceRecords
                                           .Where(ar => ar.EmployeeId == emp.Id &&
                                                        ar.Date >= payrollStartDate &&
                                                        ar.Date <= payrollEndDate &&
                                                        ar.Status == AttendanceStatus.Absent) // افترض وجود حالة للغياب
                                           .Count();
                    absenceDaysInMonth = absenceRecords;
                    */
                    // *** نهاية الإضافة ***


                    // --- تحديث منطق حساب الخصومات ---
                    decimal dailyRate = emp.BasicSalary / 30; // نفترض أن الشهر 30 يومًا لحساب معدل اليوم

                    // خصم الإجازات غير المدفوعة
                    decimal totalDeductions = (decimal)unpaidLeaveDaysInMonth * dailyRate;

                    // *** بداية الإضافة: إضافة خصم الغياب إلى الإجمالي ***
                    totalDeductions += (decimal)absenceDaysInMonth * dailyRate;
                    // *** نهاية الإضافة ***


                    decimal totalAllowances = emp.HousingAllowance + emp.TransportationAllowance;
                    decimal netSalary = emp.BasicSalary + totalAllowances - totalDeductions;

                    _payslips.Add(new PayslipViewModel
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = $"{emp.FirstName} {emp.LastName}",
                        BasicSalary = emp.BasicSalary,
                        Allowances = totalAllowances,
                        UnpaidLeaveDays = unpaidLeaveDaysInMonth,
                        AbsenceDays = absenceDaysInMonth, // <-- إضافة قيمة الغياب هنا
                        Deductions = totalDeductions,
                        NetSalary = netSalary
                    });
                }
                PayslipsDataGrid.ItemsSource = null;
                PayslipsDataGrid.ItemsSource = _payslips;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_payslips.Any()) { MessageBox.Show("يرجى احتساب الرواتب أولاً."); return; }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var companyInfo = db.CompanyInfos.FirstOrDefault();
                    if (companyInfo?.DefaultPayrollExpenseAccountId == null || companyInfo?.DefaultPayrollAccrualAccountId == null)
                    {
                        throw new Exception("يرجى تحديد حساب 'مصروف الرواتب' و 'الرواتب المستحقة' الافتراضي في شاشة الإعدادات.");
                    }

                    var payroll = new Payroll
                    {
                        Year = (int)YearComboBox.SelectedItem,
                        Month = (int)MonthComboBox.SelectedItem,
                        DateProcessed = DateTime.Today,
                        TotalAmount = _payslips.Sum(p => p.NetSalary)
                    };
                    db.Payrolls.Add(payroll);
                    db.SaveChanges();

                    foreach (var slipVM in _payslips)
                    {
                        db.Payslips.Add(new Payslip
                        {
                            PayrollId = payroll.Id,
                            EmployeeId = slipVM.EmployeeId,
                            BasicSalary = slipVM.BasicSalary,
                            Allowances = slipVM.Allowances,
                            Deductions = slipVM.Deductions,
                            NetSalary = slipVM.NetSalary
                        });
                    }

                    var journalVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"PAYROLL-{payroll.Year}-{payroll.Month}",
                        VoucherDate = payroll.DateProcessed,
                        Description = $"قيد إثبات رواتب شهر {payroll.Month} / {payroll.Year}",
                        TotalDebit = payroll.TotalAmount,
                        TotalCredit = payroll.TotalAmount,
                        Status = VoucherStatus.Posted
                    };

                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultPayrollExpenseAccountId.Value, Debit = payroll.TotalAmount, Credit = 0 });
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultPayrollAccrualAccountId.Value, Debit = 0, Credit = payroll.TotalAmount });

                    db.JournalVouchers.Add(journalVoucher);

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم حفظ مسير الرواتب والقيد المحاسبي بنجاح.", "نجاح");
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
                }
            }
        }
    }
}
