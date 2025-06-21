// Core/Services/HRService.cs
// *** الكود الكامل للتنفيذ الفعلي لخدمة الموارد البشرية بعد إضافة دوال الرواتب ***

using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System; // <-- إضافة جديدة
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class HRService : IHRService
    {
        #region Employee Management
        public async Task<List<Employee>> GetEmployeesAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Employees.AsNoTracking().ToListAsync();
            }
        }

        public async Task<Employee> GetEmployeeByIdAsync(int employeeId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Employees.FindAsync(employeeId);
            }
        }

        public async Task SaveEmployeeAsync(Employee employee)
        {
            using (var db = new DatabaseContext())
            {
                if (employee.Id == 0)
                {
                    db.Employees.Add(employee);
                }
                else
                {
                    db.Employees.Update(employee);
                }
                await db.SaveChangesAsync();
            }
        }
        #endregion

        #region Leave Type Management
        public async Task<List<LeaveType>> GetLeaveTypesAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.LeaveTypes.AsNoTracking().ToListAsync();
            }
        }

        public async Task<LeaveType> GetLeaveTypeByIdAsync(int leaveTypeId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.LeaveTypes.FindAsync(leaveTypeId);
            }
        }

        public async Task SaveLeaveTypeAsync(LeaveType leaveType)
        {
            using (var db = new DatabaseContext())
            {
                if (leaveType.Id == 0)
                {
                    db.LeaveTypes.Add(leaveType);
                }
                else
                {
                    db.LeaveTypes.Update(leaveType);
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteLeaveTypeAsync(int leaveTypeId)
        {
            using (var db = new DatabaseContext())
            {
                var leaveTypeToDelete = await db.LeaveTypes.FindAsync(leaveTypeId);
                if (leaveTypeToDelete != null)
                {
                    db.LeaveTypes.Remove(leaveTypeToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }
        #endregion

        #region Leave Request Management
        public async Task<List<LeaveRequest>> GetLeaveRequestsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.LeaveRequests
                    .Include(lr => lr.Employee)
                    .Include(lr => lr.LeaveType)
                    .AsNoTracking()
                    .OrderByDescending(lr => lr.StartDate)
                    .ToListAsync();
            }
        }

        public async Task AddLeaveRequestAsync(LeaveRequest leaveRequest)
        {
            using (var db = new DatabaseContext())
            {
                db.LeaveRequests.Add(leaveRequest);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateLeaveRequestStatusAsync(int leaveRequestId, LeaveRequestStatus newStatus)
        {
            using (var db = new DatabaseContext())
            {
                var request = await db.LeaveRequests.FindAsync(leaveRequestId);
                if (request != null)
                {
                    request.Status = newStatus;
                    await db.SaveChangesAsync();
                }
            }
        }
        #endregion

        #region Attendance Management (الكود المضاف في هذه المرحلة)
        public async Task<List<AttendanceSummaryViewModel>> GetAttendanceSummaryAsync(DateTime date)
        {
            using (var db = new DatabaseContext())
            {
                var records = await db.AttendanceRecords
                    .Include(r => r.Employee)
                    .Where(r => r.Timestamp.Date == date.Date)
                    .ToListAsync();

                var groupedRecords = records
                    .GroupBy(r => r.Employee)
                    .Select(g =>
                    {
                        var timeIn = g.Where(r => r.RecordType == RecordType.In).Min(r => (DateTime?)r.Timestamp);
                        var timeOut = g.Where(r => r.RecordType == RecordType.Out).Max(r => (DateTime?)r.Timestamp);
                        var hoursWorked = (timeOut.HasValue && timeIn.HasValue) ? (timeOut.Value - timeIn.Value).TotalHours : 0;

                        return new AttendanceSummaryViewModel
                        {
                            EmployeeName = $"{g.Key.FirstName} {g.Key.LastName}",
                            Date = date,
                            TimeIn = timeIn?.ToString("T") ?? "لم يسجل",
                            TimeOut = timeOut?.ToString("T") ?? "لم يسجل",
                            HoursWorked = hoursWorked.ToString("F2"),
                            Status = (timeIn.HasValue && timeOut.HasValue) ? "مكتمل" : (timeIn.HasValue ? "لم يسجل انصراف" : "غائب")
                        };
                    })
                    .ToList();

                return groupedRecords;
            }
        }

        public async Task AddAttendanceRecordAsync(AttendanceRecord record)
        {
            using (var db = new DatabaseContext())
            {
                db.AttendanceRecords.Add(record);
                await db.SaveChangesAsync();
            }
        }
        #endregion

        #region Payroll Management (الكود المضاف في هذه المرحلة)
        public async Task<List<Payroll>> GetPayrollsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Payrolls.AsNoTracking()
                    .OrderByDescending(p => p.Year)
                    .ThenByDescending(p => p.Month)
                    .ToListAsync();
            }
        }

        public async Task<List<PayslipViewModel>> CalculatePayslipsAsync(int year, int month)
        {
            var payslips = new List<PayslipViewModel>();
            using (var db = new DatabaseContext())
            {
                bool payrollExists = await db.Payrolls.AnyAsync(p => p.Year == year && p.Month == month);
                if (payrollExists)
                {
                    throw new Exception($"مسير الرواتب لشهر {month}/{year} موجود بالفعل.");
                }

                var employees = await db.Employees.Where(emp => emp.Status == EmployeeStatus.Active).ToListAsync();
                var payrollStartDate = new DateTime(year, month, 1);
                var payrollEndDate = payrollStartDate.AddMonths(1).AddDays(-1);

                foreach (var emp in employees)
                {
                    double unpaidLeaveDaysInMonth = 0;
                    var unpaidLeaveRequests = await db.LeaveRequests
                        .Include(lr => lr.LeaveType)
                        .Where(lr => lr.EmployeeId == emp.Id &&
                                     lr.Status == LeaveRequestStatus.Approved &&
                                     lr.LeaveType.IsPaid == false &&
                                     lr.StartDate <= payrollEndDate &&
                                     lr.EndDate >= payrollStartDate)
                        .ToListAsync();

                    foreach (var leave in unpaidLeaveRequests)
                    {
                        var leaveStart = leave.StartDate > payrollStartDate ? leave.StartDate : payrollStartDate;
                        var leaveEnd = leave.EndDate < payrollEndDate ? leave.EndDate : payrollEndDate;
                        unpaidLeaveDaysInMonth += (leaveEnd - leaveStart).TotalDays + 1;
                    }

                    int totalDaysInMonth = DateTime.DaysInMonth(year, month);
                    var attendanceDays = await db.AttendanceRecords
                        .Where(ar => ar.EmployeeId == emp.Id && ar.Timestamp.Year == year && ar.Timestamp.Month == month)
                        .Select(ar => ar.Timestamp.Date)
                        .Distinct()
                        .CountAsync();
                    double absenceDaysInMonth = totalDaysInMonth - attendanceDays - unpaidLeaveDaysInMonth;
                    if (absenceDaysInMonth < 0) absenceDaysInMonth = 0;

                    decimal dailyRate = emp.BasicSalary / 30;
                    decimal totalDeductions = ((decimal)unpaidLeaveDaysInMonth + (decimal)absenceDaysInMonth) * dailyRate;
                    decimal totalAllowances = emp.HousingAllowance + emp.TransportationAllowance;
                    decimal netSalary = emp.BasicSalary + totalAllowances - totalDeductions;

                    payslips.Add(new PayslipViewModel
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = $"{emp.FirstName} {emp.LastName}",
                        BasicSalary = emp.BasicSalary,
                        Allowances = totalAllowances,
                        UnpaidLeaveDays = unpaidLeaveDaysInMonth,
                        AbsenceDays = absenceDaysInMonth,
                        Deductions = totalDeductions,
                        NetSalary = netSalary > 0 ? netSalary : 0
                    });
                }
            }
            return payslips;
        }

        public async Task ProcessAndSavePayrollAsync(Payroll payroll, List<PayslipViewModel> payslips)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultPayrollExpenseAccountId == null || companyInfo?.DefaultPayrollAccrualAccountId == null)
                    {
                        throw new Exception("يرجى تحديد حساب 'مصروف الرواتب' و 'الرواتب المستحقة' في شاشة الإعدادات.");
                    }

                    db.Payrolls.Add(payroll);
                    await db.SaveChangesAsync();

                    foreach (var slipVM in payslips)
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
                        Description = $"قيد إثبات رواتب شهر {payroll.Month}/{payroll.Year}",
                        TotalDebit = payroll.TotalAmount,
                        TotalCredit = payroll.TotalAmount,
                        Status = VoucherStatus.Posted
                    };
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultPayrollExpenseAccountId.Value, Debit = payroll.TotalAmount, Credit = 0 });
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultPayrollAccrualAccountId.Value, Debit = 0, Credit = payroll.TotalAmount });
                    db.JournalVouchers.Add(journalVoucher);

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        #endregion
    }
}