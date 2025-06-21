// GoodMorningFactory/Core/Services/SystemInfoService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة معلومات الشركة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class SystemInfoService : ISystemInfoService
    {
        public async Task<CompanyInfo> GetCompanyInfoAsync()
        {
            using (var db = new DatabaseContext())
            {
                // استخدام FirstOrDefaultAsync لجلب أول سجل أو القيمة الافتراضية (null)
                return await db.CompanyInfos.FirstOrDefaultAsync();
            }
        }
    }
}