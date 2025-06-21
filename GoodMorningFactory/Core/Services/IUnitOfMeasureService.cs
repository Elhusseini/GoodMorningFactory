// GoodMorningFactory/Core/Services/IUnitOfMeasureService.cs
// *** ملف جديد: واجهة خدمة لوحدة إدارة وحدات القياس ***
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IUnitOfMeasureService
    {
        /// <summary>
        /// جلب قائمة بجميع وحدات القياس.
        /// </summary>
        Task<List<UnitOfMeasure>> GetUomsAsync();

        /// <summary>
        /// جلب وحدة قياس واحدة بواسطة معرفها.
        /// </summary>
        Task<UnitOfMeasure> GetUomByIdAsync(int uomId);

        /// <summary>
        /// حفظ (إضافة أو تحديث) وحدة قياس.
        /// </summary>
        Task SaveUomAsync(UnitOfMeasure uom);

        /// <summary>
        /// حذف وحدة قياس بعد التحقق من عدم استخدامها.
        /// </summary>
        Task DeleteUomAsync(int uomId);
    }
}