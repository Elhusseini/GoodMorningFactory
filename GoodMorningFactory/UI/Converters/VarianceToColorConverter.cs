// UI/Converters/VarianceToColorConverter.cs
// *** ملف جديد: محول لتلوين قيم الانحراف في تقرير الموازنة ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GoodMorningFactory.UI.Converters
{
    public class VarianceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BudgetVsActualViewModel vm)
            {
                // إذا كان الحساب مصروف، فالانحراف الموجب (صرف أكثر) سيء (أحمر)
                if (vm.AccountType == AccountType.Expense)
                {
                    return vm.Variance > 0 ? Brushes.Red : Brushes.Green;
                }
                // إذا كان الحساب إيراد، فالانحراف الموجب (تحصيل أكثر) جيد (أخضر)
                else if (vm.AccountType == AccountType.Revenue)
                {
                    return vm.Variance >= 0 ? Brushes.Green : Brushes.Red;
                }
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
