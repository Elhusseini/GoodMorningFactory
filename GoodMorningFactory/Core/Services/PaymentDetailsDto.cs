namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كائن موحد لنقل تفاصيل الدفعة من الخدمة إلى ViewModel.
    /// </summary>
    public class PaymentDetailsDto
    {
        public string InvoiceNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PreviouslyPaid { get; set; }
        public decimal TotalReturned { get; set; }
        public decimal BalanceDue { get; set; }
    }
}