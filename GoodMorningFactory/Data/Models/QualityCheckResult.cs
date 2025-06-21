// Data/Models/QualityCheckResult.cs
// *** ملف جديد: يمثل نتيجة فحص معيار واحد ضمن عملية فحص كاملة ***

using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    /// <summary>
    /// يسجل النتيجة الفعلية لمعيار فحص واحد.
    /// </summary>
    public class QualityCheckResult
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ربط مع عملية الفحص الرئيسية.
        /// </summary>
        [Required]
        public int QualityCheckId { get; set; }
        public virtual QualityCheck QualityCheck { get; set; }

        /// <summary>
        /// ربط مع المعيار الذي تم قياسه.
        /// </summary>
        [Required]
        public int QualityParameterId { get; set; }
        public virtual QualityParameter QualityParameter { get; set; }

        /// <summary>
        /// القيمة الفعلية التي تم تسجيلها أثناء الفحص.
        /// </summary>
        [Required]
        public string ActualValue { get; set; }

        /// <summary>
        /// يحدد ما إذا كانت النتيجة مطابقة للمواصفات أم لا.
        /// </summary>
        [Required]
        public bool IsConforming { get; set; }

        /// <summary>
        /// أي ملاحظات إضافية سجلها فاحص الجودة.
        /// </summary>
        public string? Notes { get; set; }
    }
}