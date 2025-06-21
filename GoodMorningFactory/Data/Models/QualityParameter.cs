// Data/Models/QualityParameter.cs
// *** ملف جديد: يمثل معيار فحص جودة واحد (مثل: اللون، الوزن) ***

using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    /// <summary>
    /// يحدد نوع القيمة المتوقعة لمعيار الفحص.
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// يتطلب قيمة رقمية (مثل الوزن).
        /// </summary>
        Numeric,
        /// <summary>
        /// يتطلب نصاً (مثل ملاحظات اللون).
        /// </summary>
        Text,
        /// <summary>
        /// يتطلب اختيار بين نجاح أو فشل.
        /// </summary>
        PassFail
    }

    /// <summary>
    /// يمثل معيار فحص جودة يمكن تطبيقه على المنتجات.
    /// </summary>
    public class QualityParameter
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// اسم المعيار (مثال: "اللزوجة").
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// وصف تفصيلي للمعيار وكيفية قياسه.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// القيمة أو النطاق المتوقع للنجاح (مثال: "مطابق للعيّنة" أو "بين 5.5 و 6.0").
        /// </summary>
        [MaxLength(200)]
        public string? ExpectedValue { get; set; }

        /// <summary>
        /// نوع المعيار لتحديد طريقة إدخال النتيجة.
        /// </summary>
        [Required]
        public ParameterType Type { get; set; }
    }
}