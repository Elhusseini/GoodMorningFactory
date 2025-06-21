// GoodMorningFactory/Core/Services/IDepartmentService.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف "العقد" أو مجموعة العمليات المتعلقة بالأقسام.
    /// أي كلاس سيقوم بتنفيذ هذه الواجهة يجب أن يوفر هذه الوظائف.
    /// </summary>
    public interface IDepartmentService
    {
        /// <summary>
        /// جلب قائمة من الأقسام مع دعم للترقيم والبحث.
        /// </summary>
        Task<PaginatedResult<DepartmentViewModel>> GetDepartmentsAsync(DepartmentFilterCriteria criteria);

        /// <summary>
        /// جلب قسم واحد بواسطة معرفه.
        /// </summary>
        Task<Department> GetDepartmentByIdAsync(int departmentId);

        /// <summary>
        /// إضافة قسم جديد إلى قاعدة البيانات.
        /// </summary>
        Task AddDepartmentAsync(Department department);

        /// <summary>
        /// تحديث بيانات قسم موجود.
        /// </summary>
        Task UpdateDepartmentAsync(Department department);

        /// <summary>
        /// حذف قسم من قاعدة البيانات.
        /// </summary>
        Task DeleteDepartmentAsync(int departmentId);

        /// <summary>
        /// الحصول على الرقم التعريفي التالي لقسم جديد.
        /// </summary>
        Task<int> GetNextDepartmentIdAsync();
    }
}
