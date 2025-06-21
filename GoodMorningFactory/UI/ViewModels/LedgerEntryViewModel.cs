using GoodMorningFactory.Core.Services;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل قيدًا واحدًا في دفتر الأستاذ.
    /// </summary>
    public class LedgerEntryViewModel : BaseViewModel
    {
        public DateTime Date { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }

        // خصائص منسقة لعرض العملة بشكل صحيح
        public string DebitFormatted => $"{Debit:N2} {AppSettings.DefaultCurrencySymbol}";
        public string CreditFormatted => $"{Credit:N2} {AppSettings.DefaultCurrencySymbol}";
        public string BalanceFormatted => $"{Balance:N2} {AppSettings.DefaultCurrencySymbol}";
    }
}
