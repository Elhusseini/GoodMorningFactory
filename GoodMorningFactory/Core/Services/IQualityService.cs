// Core/Services/IQualityService.cs
// *** ملف جديد: واجهة خدمة وحدة إدارة الجودة ***

using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// الواجهة التي تحدد العقد الخاص بكل عمليات إدارة الجودة.
    /// </summary>
    public interface IQualityService
    {
        #region Quality Parameter Management
        /// <summary>
        /// جلب قائمة بجميع معايير الفحص المعرّفة.
        /// </summary>
        Task<List<QualityParameter>> GetQualityParametersAsync();

        /// <summary>
        /// جلب معيار فحص واحد عن طريق هويته (ID).
        /// </summary>
        Task<QualityParameter> GetQualityParameterByIdAsync(int parameterId);

        /// <summary>
        /// حفظ (إضافة أو تحديث) معيار فحص.
        /// </summary>
        Task SaveQualityParameterAsync(QualityParameter parameter);

        /// <summary>
        /// حذف معيار فحص.
        /// </summary>
        Task DeleteQualityParameterAsync(int parameterId);
        #endregion

        #region Quality Check Management
        /// <summary>
        /// جلب قائمة بجميع عمليات الفحص التي تمت مع بيانات المنتج المرتبط بها.
        /// </summary>
        Task<List<QualityCheck>> GetQualityChecksAsync();

        /// <summary>
        /// جلب عملية فحص واحدة بكامل تفاصيلها ونتائجها.
        /// </summary>
        Task<QualityCheck> GetQualityCheckByIdAsync(int checkId);

        /// <summary>
        /// حفظ (إضافة أو تحديث) عملية فحص كاملة بنتائجها.
        /// </summary>
        Task SaveQualityCheckAsync(QualityCheck qualityCheck);
        #endregion
    }
}