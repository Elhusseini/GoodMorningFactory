using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GoodMorningFactory.UI.Converters
{
    /// <summary>
    /// يقوم بتحويل القيمة المنطقية إلى سماكة الخط (Bold/Normal).
    /// </summary>
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBold && isBold)
            {
                return FontWeights.Bold;
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
