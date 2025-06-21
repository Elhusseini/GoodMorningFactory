using GoodMorningFactory.Data;
using System.Linq;

// *** بداية التعديل: استخدام الـ namespace الصحيح ***
namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كلاس ثابت للوصول إلى الإعدادات العامة للتطبيق بسهولة.
    /// هذا هو المصدر الوحيد لرمز العملة الافتراضي في النظام بأكمله.
    /// </summary>
    public static class AppSettings
    {
        // تم تعيين القيمة الافتراضية إلى رمز الجنيه المصري بناء على ملاحظاتك
        public static string DefaultCurrencySymbol { get; private set; } = "ج.م";

        /// <summary>
        /// يجب استدعاء هذه الدالة عند بدء تشغيل التطبيق وفي كل مرة يتم فيها حفظ الإعدادات العامة.
        /// تقوم بتحميل رمز العملة الافتراضي المحدد من قبل المستخدم من قاعدة البيانات.
        /// </summary>
        public static void LoadSettings()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // البحث عن العملة المحددة كافتراضية
                    var defaultCurrency = db.Currencies.FirstOrDefault(c => c.IsDefault);
                    if (defaultCurrency != null)
                    {
                        DefaultCurrencySymbol = defaultCurrency.Symbol;
                    }
                    else
                    {
                        // في حال عدم تحديد أي عملة افتراضية، يتم استخدام أول عملة نشطة
                        var firstCurrency = db.Currencies.FirstOrDefault(c => c.IsActive);
                        if (firstCurrency != null)
                        {
                            DefaultCurrencySymbol = firstCurrency.Symbol;
                        }
                    }
                }
            }
            catch
            {
                // في حال حدوث أي خطأ في الاتصال بقاعدة البيانات عند بدء التشغيل،
                // سيتم استخدام الرمز الافتراضي لمنع انهيار البرنامج.
            }
        }
    }
}
