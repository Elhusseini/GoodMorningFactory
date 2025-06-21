// GoodMorningFactory/UI/Converters/BooleanToVisibilityConverter.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GoodMorningFactory.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (value is bool b) && b;

            // --- بداية الإصلاح: إضافة منطق لعكس النتيجة ---
            // هذا الكود يتحقق إذا كانت الواجهة قد أرسلت كلمة "invert"
            if (parameter != null && parameter.ToString().Equals("invert", StringComparison.OrdinalIgnoreCase))
            {
                // إذا كان الأمر كذلك، يتم عكس القيمة المنطقية
                boolValue = !boolValue;
            }
            // --- نهاية الإصلاح ---

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // لا نحتاج إلى هذا الجزء
            throw new NotImplementedException();
        }
    }
}