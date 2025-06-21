// GoodMorningFactory/Core/Services/ISalesReturnService.cs

// --- ملاحظة: هذا الملف هو "العقد" أو "الواجهة" لخدمة مرتجعات المبيعات. ---
// --- هو يحدد الوظائف التي يجب أن يوفرها أي كلاس يطبق هذه الواجهة، دون الخوض في تفاصيل التنفيذ. ---
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface ISalesReturnService
    {
        /// <summary>
        /// جلب قائمة بمرتجعات المبيعات مع الفلاتر والترقيم.
        /// </summary>
        Task<PagedResult<SalesReturn>> GetPagedSalesReturnsAsync(int page, int pageSize, string searchText, System.DateTime? fromDate, System.DateTime? toDate);

        /// <summary>
        /// جلب البيانات اللازمة لإنشاء مرتجع مبيعات من فاتورة محددة.
        /// </summary>
        Task<Sale> GetDataForReturnCreationAsync(int saleId);

        /// <summary>
        /// جلب الكميات المرتجعة سابقاً لفاتورة معينة لمنع إرجاع كمية أكبر من المشتراة.
        /// </summary>
        Task<Dictionary<int, int>> GetPreviouslyReturnedQuantitiesAsync(int saleId);

        /// <summary>
        /// حفظ مرتجع مبيعات جديد مع تحديث المخزون وإنشاء القيود المحاسبية.
        /// </summary>
        Task CreateSalesReturnAsync(int saleId, IEnumerable<SalesReturnItemViewModel> itemsToReturn);

        /// <summary>
        /// يجهز ويطبع إشعار دائن (مرتجع مبيعات).
        /// </summary>
        /// <param name="salesReturnId">معرف المرتجع المطلوب طباعته.</param>
        Task PrintCreditNoteAsync(int salesReturnId);
    }
}