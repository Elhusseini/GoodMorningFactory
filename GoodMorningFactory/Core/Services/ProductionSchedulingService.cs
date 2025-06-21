// GoodMorningFactory/Core/Services/ProductionSchedulingService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة جدولة الإنتاج ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class ProductionSchedulingService : IProductionSchedulingService
    {
        public async Task<List<WorkOrder>> GetOpenWorkOrdersAsync()
        {
            using (var db = new DatabaseContext())
            {
                var openStatuses = new[] { WorkOrderStatus.Planned, WorkOrderStatus.InProgress, WorkOrderStatus.OnHold };

                return await db.WorkOrders
                    .Include(wo => wo.FinishedGood)
                    .Where(wo => openStatuses.Contains(wo.Status))
                    .OrderBy(wo => wo.PlannedStartDate)
                    .ToListAsync();
            }
        }
    }
}