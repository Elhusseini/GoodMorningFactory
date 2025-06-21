// GoodMorningFactory/Core/Services/AppSettings.cs
using GoodMorningFactory.Data;
using System.Linq;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كلاس ثابت للوصول إلى الإعدادات العامة للتطبيق.
    /// </summary>
    public static class AppSettings
    {
        // تم استخدام الأسماء الصحيحة للخصائص مع قيم افتراضية
        public static string DefaultCurrencySymbol { get; private set; } = "ج.م";
        public static string DefaultCurrencyName_AR { get; private set; } = "جنيه مصري";
        public static string DefaultFractionalUnit_AR { get; private set; } = "قرش";

        /// <summary>
        /// تقوم بتحميل إعدادات العملة الافتراضية من قاعدة البيانات.
        /// </summary>
        public static void LoadSettings()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var defaultCurrency = db.Currencies.FirstOrDefault(c => c.IsDefault && c.IsActive);
                    if (defaultCurrency == null)
                    {
                        defaultCurrency = db.Currencies.FirstOrDefault(c => c.IsActive);
                    }

                    if (defaultCurrency != null)
                    {
                        // تحديث كل إعدادات العملة بالأسماء الصحيحة من نموذج البيانات
                        DefaultCurrencySymbol = defaultCurrency.Symbol;
                        DefaultCurrencyName_AR = defaultCurrency.CurrencyName_AR;
                        DefaultFractionalUnit_AR = defaultCurrency.FractionalUnit_AR;
                    }
                }
            }
            catch
            {
                // في حال حدوث أي خطأ، تبقى القيم الافتراضية
            }
        }
    }
}