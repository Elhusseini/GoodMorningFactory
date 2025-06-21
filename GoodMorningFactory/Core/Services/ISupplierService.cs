// GoodMorningFactory/Core/Services/ISupplierService.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف "العقد" الكامل لجميع العمليات المتعلقة بالموردين.
    /// </summary>
    public interface ISupplierService
    {
        /// <summary>
        /// جلب قائمة الموردين مع دعم للترقيم والبحث والفلترة.
        /// </summary>
        Task<PaginatedResult<SupplierViewModel>> GetSuppliersAsync(SupplierFilterCriteria criteria);

        /// <summary>
        /// جلب مورد واحد بواسطة معرفه.
        /// </summary>
        Task<Supplier> GetSupplierByIdAsync(int supplierId);

        /// <summary>
        /// إضافة مورد جديد.
        /// </summary>
        Task AddSupplierAsync(Supplier supplier);

        /// <summary>
        /// تحديث بيانات مورد موجود.
        /// </summary>
        Task UpdateSupplierAsync(Supplier supplier);

        /// <summary>
        /// حذف مورد بعد التحقق من عدم وجود ارتباطات.
        /// </summary>
        Task DeleteSupplierAsync(int supplierId);

        /// <summary>
        /// إنشاء كود جديد للمورد.
        /// </summary>
        Task<string> GetNextSupplierCodeAsync();

        /// <summary>
        /// جلب بيانات كشف حساب المورد.
        /// </summary>
        Task<List<SupplierStatementItemViewModel>> GetSupplierStatementAsync(int supplierId);
    }
}
