using System;
using System.Windows.Controls;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة لخدمة تدير التنقل بين الواجهات (Views) في التطبيق
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// حدث يتم إطلاقه عندما يتم طلب التنقل إلى واجهة جديدة
        /// </summary>
        event Action<UserControl> NavigationRequested;

        /// <summary>
        /// دالة لطلب التنقل إلى واجهة محددة باستخدام اسمها
        /// </summary>
        /// <param name="viewName">الاسم البرمجي للواجهة المطلوب عرضها</param>
        void NavigateTo(string viewName);
    }
}