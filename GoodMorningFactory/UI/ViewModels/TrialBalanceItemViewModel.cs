// UI/ViewModels/TrialBalanceItemViewModel.cs
// *** ملف جديد: ViewModel خاص ببيانات ميزان المراجعة ***
namespace GoodMorningFactory.UI.ViewModels
{
    public class TrialBalanceItemViewModel
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
    }
}