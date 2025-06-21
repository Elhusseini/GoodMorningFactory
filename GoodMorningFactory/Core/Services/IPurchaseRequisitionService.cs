// GoodMorningFactory/Core/Services/IPurchaseRequisitionService.cs
using GoodMorningFactory.Data.Models;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IPurchaseRequisitionService
    {
        /// <summary>
        /// جلب قائمة بطلبات الشراء مع الفلترة والترقيم.
        /// </summary>
        Task<PagedResult<PurchaseRequisition>> GetRequisitionsAsync(int page, int pageSize, string searchText, RequisitionStatus? status);

        /// <summary>
        /// جلب طلب شراء واحد بكامل تفاصيله.
        /// </summary>
        Task<PurchaseRequisition> GetRequisitionByIdAsync(int requisitionId);

        /// <summary>
        /// حفظ (إنشاء أو تحديث) طلب شراء.
        /// </summary>
        Task SaveRequisitionAsync(PurchaseRequisition requisition);

        /// <summary>
        /// إرسال طلب شراء للموافقة وتحديث حالته.
        /// </summary>
        Task SubmitForApprovalAsync(int requisitionId);
    }
}