// GoodMorningFactory/Data/Models/CostCenter.cs
// *** الكود الكامل والمصحح - تم تعديل خاصية الوصف ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    [Table("CostCenters")]
    public class CostCenter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // ======================= بداية الإصلاح =======================
        /// <summary>
        /// تم تحديث الحقل ليكون مطلوبًا ويتوافق مع قاعدة البيانات
        /// وتم إعطاؤه قيمة افتراضية لمنع الأخطاء.
        /// </summary>
        [Required]
        public string Description { get; set; } = string.Empty;
        // ======================== نهاية الإصلاح ========================

        public bool IsActive { get; set; } = true;
    }
}