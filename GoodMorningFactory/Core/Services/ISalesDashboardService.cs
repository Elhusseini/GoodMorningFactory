// GoodMorningFactory/Core/Services/ISalesDashboardService.cs
// *** ملف جديد: واجهة خدمة لوحة معلومات المبيعات ***
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    // كائن مساعد لنقل كل بيانات لوحة المعلومات دفعة واحدة
    public class SalesDashboardDto
    {
        public decimal TotalSalesThisMonth { get; set; }
        public int NewOrdersThisMonth { get; set; }
        public int FollowUpQuotationsCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<string> TopCustomers { get; set; }
        public List<string> TopProducts { get; set; }
        public int QuotationsCount { get; set; }
        public int OrdersCount { get; set; }
        public int InvoicesCount { get; set; }
        public Dictionary<string, decimal> MonthlySales { get; set; }
        public Dictionary<string, decimal> SalesByCategory { get; set; }
    }

    public interface ISalesDashboardService
    {
        /// <summary>
        /// جلب جميع البيانات اللازمة لعرضها في لوحة معلومات المبيعات.
        /// </summary>
        Task<SalesDashboardDto> GetDashboardDataAsync();
    }
}