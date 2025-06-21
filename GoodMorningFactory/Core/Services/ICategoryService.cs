// GoodMorningFactory/Core/Services/ICategoryService.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف "العقد" الكامل لجميع العمليات المتعلقة بالفئات.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// جلب جميع الفئات وبناء هيكل شجري منها.
        /// </summary>
        Task<ObservableCollection<CategoryViewModel>> GetCategoryTreeAsync();

        /// <summary>
        /// جلب قائمة بالفئات التي يمكن أن تكون "فئة أم".
        /// </summary>
        /// <param name="currentCategoryId">معرف الفئة الحالية (لتجنب اختيارها كأب لنفسها).</param>
        Task<List<Category>> GetPossibleParentCategoriesAsync(int? currentCategoryId);

        /// <summary>
        /// جلب فئة واحدة بواسطة معرفها.
        /// </summary>
        Task<Category> GetCategoryByIdAsync(int categoryId);

        /// <summary>
        /// إضافة فئة جديدة.
        /// </summary>
        Task AddCategoryAsync(Category category);

        /// <summary>
        /// تحديث بيانات فئة موجودة.
        /// </summary>
        Task UpdateCategoryAsync(Category category);

        /// <summary>
        /// حذف فئة بعد التحقق من عدم وجود ارتباطات.
        /// </summary>
        Task DeleteCategoryAsync(int categoryId);
    }
}
