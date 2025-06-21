// UI/ViewModels/BalanceSheetAccountViewModel.cs
// *** ملف جديد: ViewModel خاص ببيانات الميزانية العمومية لدعم العرض الشجري ***
using System.Collections.ObjectModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class BalanceSheetAccountViewModel
    {
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public ObservableCollection<BalanceSheetAccountViewModel> SubAccounts { get; set; }

        public BalanceSheetAccountViewModel()
        {
            SubAccounts = new ObservableCollection<BalanceSheetAccountViewModel>();
        }
    }
}