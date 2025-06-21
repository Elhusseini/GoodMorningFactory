namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كلاس مساعد لتمرير معايير الفلترة والترقيم لخدمة العملاء.
    /// </summary>
    public class CustomerFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string SearchText { get; set; }
        public bool? IsActive { get; set; }
    }
}
