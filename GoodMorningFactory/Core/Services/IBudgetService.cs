// GoodMorningFactory/Core/Services/IBudgetService.cs
// *** ملف جديد: واجهة خدمة شاملة لوحدة الموازنات ***
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IBudgetService
    {
        /// <summary>
        /// جلب قائمة بجميع الموازنات (بدون التفاصيل).
        /// </summary>
        Task<List<Budget>> GetBudgetsAsync();

        /// <summary>
        /// جلب موازنة واحدة مع جميع تفاصيلها وحساباتها المرتبطة.
        /// </summary>
        Task<Budget> GetBudgetWithDetailsAsync(int budgetId);

        /// <summary>
        /// جلب قائمة بجميع الحسابات التي تدخل في الموازنة (الإيرادات والمصروفات).
        /// </summary>
        Task<List<Account>> GetBudgetableAccountsAsync();

        /// <summary>
        /// حفظ (إضافة أو تحديث) الموازنة وتفاصيلها.
        /// </summary>
        Task SaveBudgetAsync(Budget budget);

        /// <summary>
        /// حذف موازنة وجميع تفاصيلها المرتبطة.
        /// </summary>
        Task DeleteBudgetAsync(int budgetId);
    }
}