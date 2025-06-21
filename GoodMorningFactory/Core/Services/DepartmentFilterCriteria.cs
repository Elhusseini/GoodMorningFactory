// GoodMorningFactory/Core/Services/DepartmentFilterCriteria.cs
namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كلاس مساعد (DTO) لتمرير معايير الفلترة والترقيم بشكل منظم إلى الخدمات.
    /// </summary>
    public class DepartmentFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15; // يمكن تعديل حجم الصفحة الافتراضي هنا
        public string SearchText { get; set; }
    }
}
