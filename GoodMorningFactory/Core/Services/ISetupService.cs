// GoodMorningFactory/Core/Services/ISetupService.cs
// *** ملف جديد: واجهة خدمة لعملية الإعداد الأولي ***
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface ISetupService
    {
        /// <summary>
        /// يقوم بإنشاء دور "مسؤول النظام" والمستخدم المدير الافتراضي.
        /// </summary>
        /// <param name="adminPassword">كلمة المرور للمدير.</param>
        Task CreateAdminUserAsync(string adminPassword);
    }
}