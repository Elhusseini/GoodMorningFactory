// UI/Views/ProcessPayrollWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة معالجة مسير الرواتب ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
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
            using (var db = new DatabaseContext())
            {
                var employees = db.Employees.Where(emp => emp.Status == EmployeeStatus.Active).ToList();
                _payslips.Clear();

                foreach (var emp in employees)
                {
                    decimal totalAllowances = emp.HousingAllowance + emp.TransportationAllowance;
                    decimal totalDeductions = 0; // سيتم إضافة منطق الخصومات لاحقاً
                    decimal netSalary = emp.BasicSalary + totalAllowances - totalDeductions;

                    _payslips.Add(new PayslipViewModel
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = $"{emp.FirstName} {emp.LastName}",
                        BasicSalary = emp.BasicSalary,
                        Allowances = totalAllowances,
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

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم حفظ مسير الرواتب بنجاح.", "نجاح");
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