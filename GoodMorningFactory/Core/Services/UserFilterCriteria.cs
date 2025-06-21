namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كلاس مساعد لتمرير معايير الفلترة والترقيم لخدمة المستخدمين.
    /// </summary>
    public class UserFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchText { get; set; }
        public bool? IsActive { get; set; }
    }
}
