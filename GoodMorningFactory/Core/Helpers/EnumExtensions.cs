using System;
using System.ComponentModel;
using System.Reflection;

namespace GoodMorningFactory.Core.Helpers
{
    /// <summary>
    /// كلاس يحتوي على دوال مساعدة (Extension Methods) للتعامل مع الـ Enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// دالة مساعدة لجلب الوصف النصي (Description Attribute) من أي قيمة Enum.
        /// </summary>
        /// <param name="value">قيمة الـ Enum.</param>
        /// <returns>الوصف النصي إذا كان موجوداً، وإلا فسيُرجع اسم القيمة كنص.</returns>
        public static string GetDescription(this Enum value)
        {
            // الحصول على معلومات الحقل الخاص بقيمة الـ Enum
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            // محاولة الحصول على [Description] attribute
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            // إرجاع الوصف أو اسم القيمة
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
