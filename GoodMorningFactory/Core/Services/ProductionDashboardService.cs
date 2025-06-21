// GoodMorningFactory/Core/Services/ProductionDashboardService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة لوحة معلومات الإنتاج ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class ProductionDashboardService : IProductionDashboardService
    {
        public async Task<ProductionDashboardDto> GetDashboardDataAsync()
        {
            using (var db = new DatabaseContext())
            {
                var today = DateTime.Today;
                var openStatuses = new[] { WorkOrderStatus.Planned, WorkOrderStatus.InProgress, WorkOrderStatus.OnHold };

                var allWorkOrders = await db.WorkOrders.Include(wo => wo.FinishedGood).AsNoTracking().ToListAsync();

                int openWorkOrders = allWorkOrders.Count(wo => openStatuses.Contains(wo.Status));
                int completedToday = allWorkOrders.Count(wo => wo.ActualEndDate.HasValue && wo.ActualEndDate.Value.Date == today);

                var completedOrders = allWorkOrders.Where(wo => wo.Status == WorkOrderStatus.Completed && wo.ActualEndDate.HasValue).ToList();
                int onTimeCount = completedOrders.Count(wo => wo.ActualEndDate.Value.Date <= wo.PlannedEndDate.Date);
                string onTimeRate = completedOrders.Any() ? $"{(double)onTimeCount / completedOrders.Count:P0}" : "N/A";

                var urgentList = allWorkOrders
                    .Where(wo => openStatuses.Contains(wo.Status) && wo.PlannedEndDate.Date <= today.AddDays(3))
                    .OrderBy(wo => wo.PlannedEndDate)
                    .ToList();

                var statusCounts = allWorkOrders
                    .GroupBy(wo => wo.Status)
                    .ToDictionary(g => GetEnumDescription(g.Key), g => g.Count());

                return new ProductionDashboardDto
                {
                    OpenWorkOrders = openWorkOrders,
                    CompletedToday = completedToday,
                    OnTimeCompletionRate = onTimeRate,
                    UrgentWorkOrders = urgentList.Count,
                    UrgentWorkOrdersList = urgentList,
                    StatusCounts = statusCounts
                };
            }
        }

        private string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }
    }
}