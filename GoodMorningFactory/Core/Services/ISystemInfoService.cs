// GoodMorningFactory/Core/Services/ISystemInfoService.cs
// *** ملف جديد: واجهة لخدمة جلب معلومات النظام والشركة ***
using GoodMorningFactory.Data.Models;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface ISystemInfoService
    {
        /// <summary>
        /// جلب معلومات الشركة من قاعدة البيانات.
        /// </summary>
        Task<CompanyInfo> GetCompanyInfoAsync();
    }
}