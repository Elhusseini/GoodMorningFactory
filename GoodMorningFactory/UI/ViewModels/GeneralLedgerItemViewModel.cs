// UI/ViewModels/GeneralLedgerItemViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض بيانات دفتر الأستاذ ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class GeneralLedgerItemViewModel
    {
        public DateTime Date { get; set; }
        public string VoucherNumber { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }
}