namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كلاس مساعد لتمرير معايير الفلترة والترقيم لخدمة الموردين.
    /// </summary>
    public class SupplierFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string SearchText { get; set; }
        public bool? IsActive { get; set; }
    }
}
