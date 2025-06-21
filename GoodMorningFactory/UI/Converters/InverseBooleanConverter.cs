// GoodMorningFactory/UI/Converters/InverseBooleanConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace GoodMorningFactory.UI.Converters
{
    /// <summary>
    /// محول يقوم بعكس القيمة المنطقية. يحول true إلى false والعكس.
    /// يستخدم في الواجهة لتعطيل العناصر عندما تكون خاصية معينة مفعلة.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return value;
        }
    }
}