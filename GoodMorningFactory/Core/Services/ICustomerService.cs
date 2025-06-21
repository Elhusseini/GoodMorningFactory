// GoodMorningFactory/Core/Services/ICustomerService.cs

// --- ملاحظة: هذا الملف يمثل "العقد" أو "الواجهة" لخدمة العملاء. ---
// --- هو يحدد أسماء ووظائف الدوال التي يجب أن يحتوي عليها أي كلاس يطبق هذه الواجهة. ---
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// الواجهة التي تحدد العمليات المتاحة لوحدة العملاء.
    /// تم تحديثها للعمل مع نماذج البيانات (Models) بدلاً من نماذج العرض (ViewModels) مباشرة.
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// إضافة عميل جديد إلى قاعدة البيانات بشكل غير متزامن.
        /// </summary>
        /// <param name="customer">كائن العميل المراد إضافته.</param>
        /// <returns>معرّف العميل الجديد بعد إضافته.</returns>
        Task<int> AddCustomerAsync(Customer customer);

        /// <summary>
        /// تحديث بيانات عميل موجود بشكل غير متزامن.
        /// </summary>
        /// <param name="customer">كائن العميل مع البيانات المحدثة.</param>
        Task UpdateCustomerAsync(Customer customer);

        /// <summary>
        /// جلب قائمة العملاء مع الترقيم والفلاتر بشكل غير متزامن.
        /// </summary>
        /// <param name="criteria">معايير البحث والترقيم.</param>
        /// <returns>نتيجة مقسمة إلى صفحات تحتوي على قائمة العملاء.</returns>
        Task<PaginatedResult<Customer>> GetCustomersAsync(CustomerFilterCriteria criteria);

        /// <summary>
        /// جلب عميل واحد بواسطة معرّفه بشكل غير متزامن.
        /// </summary>
        /// <param name="customerId">معرّف العميل.</param>
        /// <returns>كائن العميل المطابق أو null إذا لم يتم العثور عليه.</returns>
        Task<Customer> GetCustomerByIdAsync(int customerId);

        /// <summary>
        /// حذف عميل من قاعدة البيانات بشكل غير متزامن.
        /// </summary>
        /// <param name="customerId">معرّف العميل المراد حذفه.</param>
        Task DeleteCustomerAsync(int customerId);

        /// <summary>
        /// جلب قائمة بالعملاء النشطين فقط.
        /// </summary>
        /// <returns>قائمة بالعملاء النشطين.</returns>
        Task<List<Customer>> GetActiveCustomersAsync();
        Task<Dictionary<int, decimal>> GetCustomerBalancesAsync(IEnumerable<int> customerIds);
        Task<List<CustomerStatementItemViewModel>> GetCustomerStatementAsync(int customerId);

        // --- بداية الإضافة: تعريف الدالة الجديدة لتوليد الكود ---
        /// <summary>
        /// يقوم بتوليد كود العميل التالي في التسلسل (مثال: CUST-00001).
        /// </summary>
        /// <returns>كود العميل الجديد كنص.</returns>
        Task<string> GetNextCustomerCodeAsync();
        // --- نهاية الإضافة ---
    }
}