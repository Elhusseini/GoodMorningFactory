namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// فئة مركزية لتوفير الوصول إلى الخدمات المشتركة في التطبيق.
    /// هذا يتبع نمط Service Locator البسيط.
    /// </summary>
    public static class AppServices
    {
        /// <summary>
        /// يوفر نسخة فريدة من خدمة التنقل لضمان أن جميع أجزاء التطبيق
        /// تتواصل مع نفس خدمة التنقل.
        /// </summary>
        public static INavigationService NavigationService { get; private set; }

        /// <summary>
        /// دالة لتهيئة الخدمات عند بدء تشغيل التطبيق.
        /// </summary>
        public static void Initialize()
        {
            NavigationService = new NavigationService();
        }
    }
}