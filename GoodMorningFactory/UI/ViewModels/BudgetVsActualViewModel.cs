// UI/ViewModels/BudgetVsActualViewModel.cs
// *** ملف جديد: ViewModel لعرض بيانات تقرير الموازنة مقابل الفعلي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory.UI.ViewModels
{
    public class BudgetVsActualViewModel
    {
        /// <summary>
        /// نوع الحساب (إيراد أو مصروف) لتحديد منطق الانحراف.
        /// </summary>
        public AccountType AccountType { get; set; }

        /// <summary>
        /// رقم الحساب
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// اسم الحساب
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// المبلغ التقديري في الموازنة
        /// </summary>
        public decimal BudgetedAmount { get; set; }

        /// <summary>
        /// المبلغ الفعلي الذي تم صرفه أو تحصيله
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// الفرق بين الفعلي والتقديري (الإنحراف)
        /// </summary>
        public decimal Variance => ActualAmount - BudgetedAmount;

        /// <summary>
        /// نسبة الإنحراف
        /// </summary>
        public decimal VariancePercentage
        {
            get
            {
                if (BudgetedAmount == 0)
                {
                    // إذا كانت الموازنة صفراً، لا يمكن حساب النسبة
                    return ActualAmount > 0 ? 1m : 0m; // 100%
                }
                return Variance / BudgetedAmount;
            }
        }

        // --- خصائص منسقة للعرض في الواجهة ---
        public string BudgetedAmountFormatted => $"{BudgetedAmount:N2} {AppSettings.DefaultCurrencySymbol}";
        public string ActualAmountFormatted => $"{ActualAmount:N2} {AppSettings.DefaultCurrencySymbol}";
        public string VarianceFormatted => $"{Variance:N2} {AppSettings.DefaultCurrencySymbol}";
        public string VariancePercentageFormatted => VariancePercentage.ToString("P1"); // "P1" for percentage with one decimal place
    }
}
