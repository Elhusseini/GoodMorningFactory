using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العمليات المتعلقة بجلب بيانات لوحة معلومات الموارد البشرية.
    /// </summary>
    public interface IHRDashboardService
    {
        /// <summary>
        /// جلب جميع مؤشرات الأداء الرئيسية للموارد البشرية.
        /// </summary>
        Task<HRKpisDto> GetHRKpisAsync();

        /// <summary>
        /// جلب بيانات توزيع الموظفين حسب القسم للرسم البياني.
        /// </summary>
        Task<Dictionary<string, int>> GetDepartmentDistributionAsync();
    }

    /// <summary>
    /// كائن لنقل بيانات مؤشرات الأداء الرئيسية للموارد البشرية من الخدمة إلى الـ ViewModel.
    /// </summary>
    public class HRKpisDto
    {
        public int TotalActiveEmployees { get; set; }
        public int NewHiresLast30Days { get; set; }
        public int TerminationsLast30Days { get; set; }
        public int PendingLeaveRequests { get; set; }
    }
}
