using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GoodMorningFactory.UI.Converters
{
    /// <summary>
    /// يقوم هذا المحول بتحويل القيمة المنطقية (boolean) إلى لون محدد.
    /// يستخدم لعرض حالة المستخدم (نشط/غير نشط) بألوان مختلفة (أخضر/أحمر).
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        /// <summary>
        /// تحويل القيمة المنطقية إلى لون.
        /// </summary>
        /// <param name="value">القيمة المنطقية (true/false).</param>
        /// <returns>فرشاة خضراء (Green) إذا كانت القيمة true، وحمراء (Red) إذا كانت false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Brushes.Green : Brushes.IndianRed;
            }
            // في حالة وجود قيمة غير متوقعة، يتم إرجاع اللون الأسود كقيمة افتراضية
            return Brushes.Black;
        }

        /// <summary>
        /// هذا التحويل العكسي غير مطلوب في حالتنا.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
