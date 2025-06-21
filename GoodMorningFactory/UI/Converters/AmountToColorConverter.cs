using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GoodMorningFactory.UI.Converters
{
    /// <summary>
    /// يقوم بتحويل القيمة الرقمية إلى لون (أحمر للسالب، أسود للموجب).
    /// </summary>
    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                return amount < 0 ? Brushes.Red : Brushes.Black;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
