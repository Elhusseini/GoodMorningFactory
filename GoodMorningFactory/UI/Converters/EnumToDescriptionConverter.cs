// GoodMorning/UI/Converters/EnumToDescriptionConverter.cs
// *** ملف أساسي ومهم: هذا المحول (Converter) مسؤول عن قراءة الوصف العربي من أنواع البيانات (Enums) وعرضه في الواجهة ***
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace GoodMorningFactory.UI.Converters
{
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            Enum myEnum;

            if (value is Enum)
            {
                myEnum = (Enum)value;
            }
            else
            {
                return value.ToString();
            }

            return GetEnumDescription(myEnum);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty; // Not needed
        }

        // دالة مساعدة لجلب الوصف من الـ Attribute
        private string GetEnumDescription(Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
            if (fieldInfo == null) return enumObj.ToString();

            object[] attribArray = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attribArray.Length > 0)
            {
                return (attribArray[0] as DescriptionAttribute).Description;
            }
            else
            {
                return enumObj.ToString();
            }
        }
    }
}