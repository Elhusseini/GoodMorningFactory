// UI/ViewModels/IncomeStatementItemViewModel.cs
// *** ملف جديد: ViewModel خاص ببيانات قائمة الدخل ***
namespace GoodMorningFactory.UI.ViewModels
{
    public class IncomeStatementItemViewModel
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
    }
}