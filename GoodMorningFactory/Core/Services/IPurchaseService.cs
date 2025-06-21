// GoodMorningFactory/Core/Services/IPurchaseService.cs
// *** الكود الكامل والنهائي ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كائن لنقل كل البيانات اللازمة لنافذة الإنشاء/التعديل دفعة واحدة.
    /// هذا يقلل من عدد الاستدعاءات لقاعدة البيانات ويحسن الأداء.
    /// </summary>
    public class AddEditPurchaseDto
    {
        public Purchase Purchase { get; set; }
        public List<Supplier> AllSuppliers { get; set; }
        public List<Product> AllProducts { get; set; }
        public List<int> SourceGrnIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// واجهة تعرف العمليات المتعلقة بفواتير المشتريات.
    /// </summary>
    public interface IPurchaseService
    {
        /// <summary>
        /// جلب قائمة فواتير المشتريات بشكل غير متزامن مع تطبيق الفلاتر وترقيم الصفحات.
        /// </summary>
        Task<PaginatedResult<PurchaseViewModel>> GetPurchasesAsync(PurchaseFilterCriteria criteria);

        /// <summary>
        /// جلب فاتورة شراء واحدة مع تفاصيلها لغرض الطباعة.
        /// </summary>
        Task<Purchase> GetPurchaseForPrintingAsync(int purchaseId);

        // --- بداية التعديل: الدالة الآن تقبل كل أنواع المعرفات ---
        /// <summary>
        /// جلب البيانات اللازمة لإنشاء فاتورة جديدة أو تعديل فاتورة حالية.
        /// </summary>
        Task<AddEditPurchaseDto> GetDataForAddEditAsync(int? purchaseId, int? purchaseOrderId, int? grnId);
        // --- نهاية التعديل ---

        // --- بداية التعديل: الدالة الآن تقبل قائمة بمعرفات سندات الاستلام ---
        /// <summary>
        /// حفظ (إنشاء أو تحديث) فاتورة شراء مع تحديث القيد المحاسبي وحالة سندات الاستلام.
        /// </summary>
        /// <param name="purchase">كائن الفاتورة الأساسي.</param>
        /// <param name="items">قائمة البنود في الفاتورة.</param>
        /// <param name="grnIdsToInvoice">قائمة بمعرفات سندات الاستلام المستخدمة لإنشاء هذه الفاتورة.</param>
        Task SavePurchaseAsync(Purchase purchase, List<PurchaseInvoiceItemViewModel> items, List<int> grnIdsToInvoice);
        // --- نهاية التعديل ---
    }
}