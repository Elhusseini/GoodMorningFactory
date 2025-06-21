// GoodMorningFactory/Core/Services/ISalesQuotationService.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels; // لاستخدام PaginatedResult
using System;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العقد الخاص بخدمات عروض الأسعار.
    /// </summary>
    public interface ISalesQuotationService
    {
        /// <summary>
        /// جلب عروض الأسعار مع دعم للترقيم والبحث والفلترة.
        /// </summary>
        /// <param name="criteria">معايير البحث والفلترة.</param>
        /// <returns>نتيجة مقسمة إلى صفحات تحتوي على عروض الأسعار.</returns>
        Task<PaginatedResult<SalesQuotation>> GetQuotationsAsync(QuotationFilterCriteria criteria);

        /// <summary>
        /// حذف عرض سعر.
        /// </summary>
        /// <param name="quotationId">معرف عرض السعر المراد حذفه.</param>
        Task DeleteQuotationAsync(int quotationId);

        /// <summary>
        /// تحديث حالة عرض السعر (مثلاً، عند تحويله إلى أمر بيع).
        /// </summary>
        /// <param name="quotationId">معرف عرض السعر.</param>
        /// <param name="newStatus">الحالة الجديدة.</param>
        Task UpdateQuotationStatusAsync(int quotationId, QuotationStatus newStatus);
    }

    /// <summary>
    /// كلاس مساعد لتمرير معايير الفلترة والترقيم بشكل منظم.
    /// </summary>
    public class QuotationFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string SearchText { get; set; }
        public QuotationStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}