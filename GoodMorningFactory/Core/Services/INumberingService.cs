// GoodMorningFactory/Core/Services/INumberingService.cs
// الواجهة (العقد) لخدمة الترقيم التلقائي.
using GoodMorningFactory.Data.Models;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface INumberingService
    {
        /// <summary>
        /// يقوم بتوليد الرقم التالي للمستند المحدد بناءً على الإعدادات في قاعدة البيانات.
        /// </summary>
        /// <param name="documentType">نوع المستند المطلوب ترقيمه.</param>
        /// <returns>الرقم المتسلسل الجديد كنص.</returns>
        Task<string> GetNextNumberAsync(DocumentType documentType);
    }
}