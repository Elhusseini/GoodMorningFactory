// GoodMorningFactory/Core/Services/NumberingService.cs
// التنفيذ الفعلي لخدمة الترقيم التلقائي، مصمم ليعمل مع نموذج البيانات الخاص بك.
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class NumberingService : INumberingService
    {
        public async Task<string> GetNextNumberAsync(DocumentType documentType)
        {
            using (var db = new DatabaseContext())
            {
                var sequence = await db.NumberingSequences.FirstOrDefaultAsync(s => s.DocumentType == documentType);

                if (sequence == null)
                {
                    // في حال عدم وجود إعدادات لهذا المستند، يتم إنشاء رقم افتراضي
                    // يمكنك لاحقاً إنشاء واجهة لإدارة هذه الإعدادات
                    return $"{documentType.ToString().ToUpper()}-{System.DateTime.Now:yyyyMMddHHmmss}";
                }

                // زيادة آخر رقم بمقدار واحد
                sequence.LastNumber++;
                await db.SaveChangesAsync();

                // تنسيق الرقم الجديد بناءً على عدد الخانات (مثال: QUO-00001)
                string format = new string('0', sequence.NumberOfDigits);
                return $"{sequence.Prefix}-{sequence.LastNumber.ToString(format)}";
            }
        }
    }
}