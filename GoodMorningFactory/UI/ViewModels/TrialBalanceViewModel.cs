// UI/ViewModels/TrialBalanceViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض بيانات ميزان المراجعة ***
namespace GoodMorningFactory.UI.ViewModels
{
    public class TrialBalanceViewModel
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
    }
}