using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GoodMorningFactory.UI.Converters
{
    /// <summary>
    /// يقوم بتحويل المستوى الرقمي إلى مسافة بادئة (Margin) للعرض الشجري.
    /// </summary>
    public class IndentConverter : IValueConverter
    {
        public double IndentSize { get; set; } = 25.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int indentLevel)
            {
                return new Thickness(indentLevel * IndentSize, 0, 0, 0);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
