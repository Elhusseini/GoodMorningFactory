// GoodMorningFactory/UI/ViewModels/PurchaseViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel مخصص لعرض بيانات فاتورة المشتريات في الواجهة الرئيسية.
    /// </summary>
    public class PurchaseViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string SupplierName { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public PurchaseInvoiceStatus Status { get; set; }
        public decimal TotalReturned { get; set; }

        // ======================= بداية الإصلاح الرئيسي =======================
        public decimal BalanceDue => TotalAmount - AmountPaid - TotalReturned;
        // ======================== نهاية الإصلاح الرئيسي ========================

        public string TotalAmountFormatted => $"{TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
        public string AmountPaidFormatted => $"{AmountPaid:N2} {AppSettings.DefaultCurrencySymbol}";
        public string BalanceDueFormatted => $"{BalanceDue:N2} {AppSettings.DefaultCurrencySymbol}";
        public bool IsOverdue => DueDate.HasValue && DueDate.Value.Date < DateTime.Today.Date && BalanceDue > 0;
    }
}