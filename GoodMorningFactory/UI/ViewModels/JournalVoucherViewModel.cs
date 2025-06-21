using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل قيد يومي واحد للعرض في الجدول.
    /// </summary>
    public class JournalVoucherViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime VoucherDate { get; set; }
        public string Description { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public VoucherStatus Status { get; set; }

        public string TotalDebitFormatted => $"{TotalDebit:N2} {AppSettings.DefaultCurrencySymbol}";
        public string TotalCreditFormatted => $"{TotalCredit:N2} {AppSettings.DefaultCurrencySymbol}";
    }
}
