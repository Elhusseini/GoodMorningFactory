// GoodMorningFactory/Core/Services/AuditTrailService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة سجل التدقيق ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class AuditTrailService : IAuditTrailService
    {
        public async Task<List<string>> GetUsernamesForFilterAsync()
        {
            using (var db = new DatabaseContext())
            {
                var users = await db.Users
                                    .OrderBy(u => u.Username)
                                    .Select(u => u.Username)
                                    .ToListAsync();
                users.Insert(0, "الكل"); // إضافة خيار "الكل" في البداية
                return users;
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(AuditFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.AuditLogs.AsQueryable();

                if (criteria.FromDate.HasValue)
                {
                    query = query.Where(log => log.Timestamp >= criteria.FromDate.Value.Date);
                }
                if (criteria.ToDate.HasValue)
                {
                    var toDate = criteria.ToDate.Value.Date.AddDays(1);
                    query = query.Where(log => log.Timestamp < toDate);
                }
                if (!string.IsNullOrWhiteSpace(criteria.SelectedUser) && criteria.SelectedUser != "الكل")
                {
                    query = query.Where(log => log.Username == criteria.SelectedUser);
                }
                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string searchTextLower = criteria.SearchText.ToLower();
                    query = query.Where(log => log.EntityName.ToLower().Contains(searchTextLower) || log.Changes.ToLower().Contains(searchTextLower));
                }

                return await query.OrderByDescending(log => log.Timestamp).ToListAsync();
            }
        }
    }
}