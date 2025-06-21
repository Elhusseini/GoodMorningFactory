// GoodMorningFactory/Core/Services/ICostCenterService.cs
// *** ملف جديد: واجهة خدمة لوحدة إدارة مراكز التكلفة ***
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface ICostCenterService
    {
        /// <summary>
        /// جلب قائمة بجميع مراكز التكلفة.
        /// </summary>
        Task<List<CostCenter>> GetCostCentersAsync();

        /// <summary>
        /// جلب مركز تكلفة واحد بواسطة معرّفه.
        /// </summary>
        Task<CostCenter> GetCostCenterByIdAsync(int id);

        /// <summary>
        /// حفظ (إضافة أو تحديث) مركز تكلفة.
        /// </summary>
        Task SaveCostCenterAsync(CostCenter costCenter);

        /// <summary>
        /// حذف مركز تكلفة بعد التحقق من عدم استخدامه.
        /// </summary>
        Task DeleteCostCenterAsync(int id);
    }
}