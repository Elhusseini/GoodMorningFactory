// GoodMorningFactory/Core/Services/IAuditTrailService.cs
// *** ملف جديد: واجهة خدمة لوحدة سجل التدقيق ***
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    // كائن لنقل معايير الفلترة إلى الخدمة
    public class AuditFilterCriteria
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SelectedUser { get; set; }
        public string SearchText { get; set; }
    }

    public interface IAuditTrailService
    {
        /// <summary>
        /// جلب قائمة المستخدمين لعرضها في فلتر البحث.
        /// </summary>
        Task<List<string>> GetUsernamesForFilterAsync();

        /// <summary>
        /// جلب سجلات التدقيق بناءً على معايير الفلترة.
        /// </summary>
        Task<List<AuditLog>> GetAuditLogsAsync(AuditFilterCriteria criteria);
    }
}