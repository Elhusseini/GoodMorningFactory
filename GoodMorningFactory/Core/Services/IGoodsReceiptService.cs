// GoodMorningFactory/Core/Services/IGoodsReceiptService.cs
// *** الكود الكامل والمعدل ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    // --- ملاحظة: هذا الكلاس المساعد يستخدم لنقل نتائج البحث المقسمة إلى صفحات. ---
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
    }

    public interface IGoodsReceiptService
    {
        /// <summary>
        /// جلب قائمة بسندات الاستلام مع الترقيم.
        /// </summary>
        Task<PagedResult<GoodsReceiptNote>> GetPagedGoodsReceiptsAsync(int page, int pageSize);

        /// <summary>
        /// جلب البيانات اللازمة لإنشاء سند استلام جديد لأمر شراء محدد.
        /// </summary>
        Task<List<GoodsReceiptItemViewModel>> GetDataForReceiptCreationAsync(int purchaseOrderId);

        /// <summary>
        /// حفظ سند استلام جديد بكل عملياته (تحديث المخزون، القيود المحاسبية، إلخ).
        /// </summary>
             Task SaveGoodsReceiptAsync(int purchaseOrderId, IEnumerable<GoodsReceiptItemViewModel> items);
        // ======================= بداية الإضافة =======================
        /// <summary>
        /// جلب تفاصيل سند استلام واحد بواسطة معرفه.
        /// </summary>
        Task<GoodsReceiptNote> GetGoodsReceiptByIdAsync(int grnId);
        // ======================== نهاية الإضافة ========================
    }
}