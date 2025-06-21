namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// كلاس مساعد عام يمثل عنصرًا في قائمة منسدلة للفلترة.
    /// </summary>
    /// <typeparam name="T">نوع قيمة الفلتر (مثل int, bool?, enum).</typeparam>
    public class FilterItem<T>
    {
        /// <summary>
        /// النص الذي يظهر للمستخدم في القائمة.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// القيمة الفعلية التي تستخدم في الفلترة.
        /// </summary>
        public T Value { get; set; }
    }
}
