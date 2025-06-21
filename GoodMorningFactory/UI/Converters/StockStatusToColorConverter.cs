// GoodMorningFactory/UI/Converters/StockStatusToColorConverter.cs
using GoodMorningFactory.Data.Models; // مطلوب للوصول إلى StockStatus
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GoodMorningFactory.UI.Converters
{
    /// <summary>
    /// يقوم هذا المحول بتحويل قيمة StockStatus إلى لون معين لتمييز حالة المخزون.
    /// </summary>
    public class StockStatusToColorConverter : IValueConverter
    {
        /// <summary>
        /// تحويل حالة المخزون إلى لون.
        /// </summary>
        /// <param name="value">القيمة من نوع StockStatus.</param>
        /// <returns>فرشاة لون مناسبة للحالة.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StockStatus status)
            {
                switch (status)
                {
                    case StockStatus.Available:
                        return new SolidColorBrush(Colors.SeaGreen); // أخضر داكن للمتوفر
                    case StockStatus.LowStock:
                        return new SolidColorBrush(Colors.Orange); // برتقالي للمخزون المنخفض
                    case StockStatus.OutOfStock:
                        return new SolidColorBrush(Colors.IndianRed); // أحمر للذي نفد
                    default:
                        return Brushes.Gray; // رمادي للحالات غير المحددة
                }
            }
            return Brushes.Gray;
        }

        /// <summary>
        /// التحويل العكسي غير مطلوب.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}