// UI/Converters/BooleanToVisibilityConverter.cs
// هذا المحول يقوم بتحويل القيمة المنطقية (true/false) إلى حالة الظهور (Visible/Collapsed)
// وهو ضروري لعمل واجهة استلام البضاعة بشكل صحيح.

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
            if (value is bool boolValue)
            {
                // إذا كانت القيمة true، أظهر العنصر
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            // إذا كانت القيمة غير منطقية، قم بإخفاء العنصر كإجراء افتراضي
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
