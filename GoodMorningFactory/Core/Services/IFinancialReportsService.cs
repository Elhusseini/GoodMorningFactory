// GoodMorningFactory/Core/Services/IFinancialReportsService.cs
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IFinancialReportsService
    {
        Task<List<TrialBalanceItemViewModel>> GetTrialBalanceAsync(DateTime toDate);

        // --- بداية الإضافة: تعريف دالة قائمة الدخل ---
        /// <summary>
        /// يقوم بحساب وإرجاع بيانات تقرير قائمة الدخل لفترة محددة.
        /// </summary>
        /// <param name="fromDate">تاريخ بداية الفترة.</param>
        /// <param name="toDate">تاريخ نهاية الفترة.</param>
        /// <returns>ViewModel يحتوي على كل بيانات قائمة الدخل المحسوبة.</returns>
        Task<IncomeStatementViewModel> GetIncomeStatementAsync(DateTime fromDate, DateTime toDate);
        // --- نهاية الإضافة ---
    }
}