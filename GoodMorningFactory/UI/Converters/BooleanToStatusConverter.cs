// UI/Converters/BooleanToStatusConverter.cs
// *** الكود الكامل لملف المحول المشترك للتأكد من وجوده وتصحيح مرجعيته ***
using System;
using System.Globalization;
using System.Windows.Data;

namespace GoodMorningFactory.UI.Converters
{
    public class BooleanToStatusConverter : IValueConverter
    {
        // دالة لتحويل القيمة المنطقية (true/false) إلى نص مفهوم للمستخدم (نشط/غير نشط).
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? "نشط" : "غير نشط";
            }
            return string.Empty;
        }

        // هذه الدالة لا نحتاجها في حالتنا، لذا نتركها فارغة
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}