// Core/Services/QualityService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة إدارة الجودة ***

using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// الفئة التي تحتوي على المنطق الفعلي للتعامل مع قاعدة البيانات لكل ما يخص وحدة الجودة.
    /// </summary>
    public class QualityService : IQualityService
    {
        #region Quality Parameter Management
        public async Task<List<QualityParameter>> GetQualityParametersAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.QualityParameters.AsNoTracking().ToListAsync();
            }
        }

        public async Task<QualityParameter> GetQualityParameterByIdAsync(int parameterId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.QualityParameters.FindAsync(parameterId);
            }
        }

        public async Task SaveQualityParameterAsync(QualityParameter parameter)
        {
            using (var db = new DatabaseContext())
            {
                if (parameter.Id == 0)
                {
                    db.QualityParameters.Add(parameter);
                }
                else
                {
                    db.QualityParameters.Update(parameter);
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteQualityParameterAsync(int parameterId)
        {
            using (var db = new DatabaseContext())
            {
                var parameter = await db.QualityParameters.FindAsync(parameterId);
                if (parameter != null)
                {
                    db.QualityParameters.Remove(parameter);
                    await db.SaveChangesAsync();
                }
            }
        }
        #endregion

        #region Quality Check Management
        public async Task<List<QualityCheck>> GetQualityChecksAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.QualityChecks
                    .Include(qc => qc.Product) // تضمين اسم المنتج مع كل فحص
                    .AsNoTracking()
                    .OrderByDescending(qc => qc.CheckDate)
                    .ToListAsync();
            }
        }

        public async Task<QualityCheck> GetQualityCheckByIdAsync(int checkId)
        {
            using (var db = new DatabaseContext())
            {
                // جلب عملية الفحص مع كل النتائج والمعايير المرتبطة بها
                return await db.QualityChecks
                    .Include(qc => qc.Product)
                    .Include(qc => qc.Results)
                        .ThenInclude(res => res.QualityParameter)
                    .FirstOrDefaultAsync(qc => qc.Id == checkId);
            }
        }

        public async Task SaveQualityCheckAsync(QualityCheck qualityCheck)
        {
            using (var db = new DatabaseContext())
            {
                if (qualityCheck.Id == 0)
                {
                    db.QualityChecks.Add(qualityCheck);
                }
                else
                {
                    db.QualityChecks.Update(qualityCheck);
                }
                await db.SaveChangesAsync();
            }
        }
        #endregion
    }
}