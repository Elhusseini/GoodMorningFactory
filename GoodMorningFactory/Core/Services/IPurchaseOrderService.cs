// GoodMorningFactory/Core/Services/IPurchaseOrderService.cs
// *** الكود الكامل والمعدل ***
using GoodMorningFactory.Data.Models;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    // هذا الكائن سيساعدنا في نقل بيانات الإنشاء والتعديل بسهولة
    public class PurchaseOrderDto
    {
        public PurchaseOrder Order { get; set; }
        public System.Collections.Generic.List<Supplier> AllSuppliers { get; set; }
        public System.Collections.Generic.List<Product> AllProducts { get; set; }
    }

    public interface IPurchaseOrderService
    {
        /// <summary>
        /// جلب قائمة بأوامر الشراء مع الفلترة والترقيم.
        /// </summary>
        Task<PagedResult<PurchaseOrder>> GetPurchaseOrdersAsync(int page, int pageSize, string searchText, PurchaseOrderStatus? status);

        /// <summary>
        /// جلب البيانات اللازمة لإنشاء أمر شراء جديد أو تعديل أمر حالي.
        /// </summary>
        Task<PurchaseOrderDto> GetDataForAddEditAsync(int? poId);

        /// <summary>
        /// حفظ (إنشاء أو تحديث) أمر شراء.
        /// </summary>
        Task SavePurchaseOrderAsync(PurchaseOrder order);
        Task CancelPurchaseOrderAsync(int poId);

        // ======================= بداية الإضافة =======================
        /// <summary>
        /// جلب البيانات اللازمة لإنشاء أمر شراء جديد بناءً على طلب شراء موجود.
        /// </summary>
        Task<PurchaseOrderDto> GetDataForPOFromRequisitionAsync(int requisitionId);
        // ======================== نهاية الإضافة ========================
    }
}