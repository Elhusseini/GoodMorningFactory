using System.Collections.Generic;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// يمثل نتيجة مقسمة إلى صفحات.
    /// </summary>
    /// <typeparam name="T">نوع العناصر في القائمة.</typeparam>
    public class PaginatedResult<T>
    {
        /// <summary>
        /// قائمة العناصر في الصفحة الحالية.
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// العدد الإجمالي للعناصر عبر جميع الصفحات.
        /// </summary>
        public int TotalCount { get; set; }
    }
}
