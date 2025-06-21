// GoodMorningFactory/Core/Services/IFixedAssetService.cs
// *** الكود الكامل والمعدل - تم تعديل نوع الإرجاع لدالة احتساب الإهلاك ***
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IFixedAssetService
    {
        /// <summary>
        /// جلب قائمة بجميع الأصول الثابتة.
        /// </summary>
        Task<List<FixedAsset>> GetAssetsAsync();

        /// <summary>
        /// جلب أصل ثابت واحد مع الحسابات المرتبطة به.
        /// </summary>
        Task<FixedAsset> GetAssetByIdAsync(int id);

        /// <summary>
        /// جلب قائمة بالحسابات التي يمكن استخدامها كحسابات للأصول.
        /// </summary>
        Task<List<Account>> GetAssetAccountsAsync();

        /// <summary>
        /// جلب قائمة بالحسابات التي يمكن استخدامها كمصروفات إهلاك.
        /// </summary>
        Task<List<Account>> GetExpenseAccountsAsync();

        /// <summary>
        /// حفظ (إضافة أو تحديث) أصل ثابت.
        /// </summary>
        Task SaveAssetAsync(FixedAsset asset);
        Task DeleteAssetAsync(int id);

        // --- دوال المرحلة الثانية (مع التعديل) ---

        /// <summary>
        /// يقوم باحتساب قيد الإهلاك المقترح لفترة معينة دون حفظه.
        /// </summary>
        /// <param name="periodDate">تاريخ نهاية الفترة (الشهر والسنة).</param>
        /// <returns>Tuple يحتوي على القيد المقترح وتفاصيل الأصول المهلكة.</returns>
        Task<(JournalVoucher voucher, Dictionary<int, decimal> details)> CalculateDepreciationVoucherAsync(DateTime periodDate);

        /// <summary>
        /// يقوم بترحيل قيد الإهلاك المقترح وحفظ سجلات الإهلاك.
        /// </summary>
        /// <param name="proposedVoucher">كائن قيد اليومية الذي تم إنشاؤه.</param>
        /// <param name="depreciatedAssets">قاموس يحتوي على معرفات الأصول التي تم إهلاكها ومبلغ الإهلاك لكل منها.</param>
        Task PostDepreciationVoucherAsync(JournalVoucher proposedVoucher, Dictionary<int, decimal> depreciatedAssets);
    }
}