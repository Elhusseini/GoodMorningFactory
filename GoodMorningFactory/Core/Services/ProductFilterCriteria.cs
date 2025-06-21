using GoodMorningFactory.Data.Models; // Required for ProductType enum

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كلاس مساعد لتمرير معايير الفلترة والترقيم لخدمة المنتجات.
    /// تم تحديثه ليشمل جميع الفلاتر الجديدة.
    /// </summary>
    public class ProductFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string SearchText { get; set; }
        public int CategoryId { get; set; } // Changed to non-nullable int
        public int SupplierId { get; set; } // New filter
        public ProductType? ProductType { get; set; } // New filter
        public bool? IsActive { get; set; }
    }
}
