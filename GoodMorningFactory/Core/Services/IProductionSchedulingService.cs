// GoodMorningFactory/Core/Services/IProductionSchedulingService.cs
// *** ملف جديد: واجهة خدمة لجلب بيانات جدولة الإنتاج ***
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IProductionSchedulingService
    {
        /// <summary>
        /// جلب جميع أوامر العمل التي لم تكتمل بعد (مخطط لها، قيد التنفيذ، معلقة).
        /// </summary>
        /// <returns>قائمة بأوامر العمل المفتوحة.</returns>
        Task<List<WorkOrder>> GetOpenWorkOrdersAsync();
    }
}