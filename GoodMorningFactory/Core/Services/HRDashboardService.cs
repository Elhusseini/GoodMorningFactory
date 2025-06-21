using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية مسؤولة عن جلب وتجميع بيانات لوحة معلومات الموارد البشرية.
    /// </summary>
    public class HRDashboardService : IHRDashboardService
    {
        public async Task<HRKpisDto> GetHRKpisAsync()
        {
            using (var db = new DatabaseContext())
            {
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);

                var totalActiveTask = db.Employees.CountAsync(e => e.Status == EmployeeStatus.Active);
                var newHiresTask = db.Employees.CountAsync(e => e.HireDate >= thirtyDaysAgo);
                var terminationsTask = db.Employees.CountAsync(e => e.TerminationDate.HasValue && e.TerminationDate.Value >= thirtyDaysAgo);
                var pendingLeavesTask = db.LeaveRequests.CountAsync(lr => lr.Status == LeaveRequestStatus.Pending);

                await Task.WhenAll(totalActiveTask, newHiresTask, terminationsTask, pendingLeavesTask);

                return new HRKpisDto
                {
                    TotalActiveEmployees = await totalActiveTask,
                    NewHiresLast30Days = await newHiresTask,
                    TerminationsLast30Days = await terminationsTask,
                    PendingLeaveRequests = await pendingLeavesTask
                };
            }
        }

        public async Task<Dictionary<string, int>> GetDepartmentDistributionAsync()
        {
            using (var db = new DatabaseContext())
            {
                // *** بداية الإصلاح النهائي: تم تعديل الاستعلام ليتعامل مع خاصية Department كنص ***
                return await db.Employees
                    .Where(e => e.Status == EmployeeStatus.Active && !string.IsNullOrEmpty(e.Department))
                    .GroupBy(e => e.Department) // <-- التجميع يتم الآن على اسم القسم مباشرة
                    .Select(g => new { DepartmentName = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(k => k.DepartmentName, v => v.Count);
                // *** نهاية الإصلاح النهائي ***
            }
        }
    }
}
