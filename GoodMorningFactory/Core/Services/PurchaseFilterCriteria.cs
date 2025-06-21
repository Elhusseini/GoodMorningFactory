using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كلاس مساعد لتمرير معايير الفلترة والترقيم لخدمة المشتريات.
    /// </summary>
    public class PurchaseFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string SearchText { get; set; }
        public PurchaseInvoiceStatus? Status { get; set; }
    }
}
