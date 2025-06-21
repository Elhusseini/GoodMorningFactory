// GoodMorningFactory/Core/Services/IProductionDashboardService.cs
// *** ملف جديد: واجهة خدمة لوحة معلومات الإنتاج ***
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    // كائن مساعد لنقل كل بيانات لوحة المعلومات دفعة واحدة
    public class ProductionDashboardDto
    {
        public int OpenWorkOrders { get; set; }
        public int CompletedToday { get; set; }
        public string OnTimeCompletionRate { get; set; }
        public int UrgentWorkOrders { get; set; }
        public List<WorkOrder> UrgentWorkOrdersList { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; }
    }

    public interface IProductionDashboardService
    {
        /// <summary>
        /// جلب جميع البيانات اللازمة لعرضها في لوحة معلومات الإنتاج.
        /// </summary>
        Task<ProductionDashboardDto> GetDashboardDataAsync();
    }
}