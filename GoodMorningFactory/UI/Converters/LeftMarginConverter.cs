// GoodMorningFactory/UI/Converters/LeftMarginConverter.cs
// *** ملف جديد: محول مخصص لتحويل المسافة اليسرى للمهام في مخطط جانت ***
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GoodMorningFactory.UI.Converters
{
    public class LeftMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double leftOffset)
            {
                return new Thickness(leftOffset, 0, 0, 0);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}