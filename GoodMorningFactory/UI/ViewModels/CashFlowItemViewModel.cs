// UI/ViewModels/CashFlowItemViewModel.cs
// *** ملف جديد: ViewModel خاص ببنود قائمة التدفقات النقدية ***

namespace GoodMorningFactory.UI.ViewModels
{
    public class CashFlowItemViewModel
    {
        /// <summary>
        /// الفئة الرئيسية للبند (تشغيلية، استثمارية، تمويلية)
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// وصف البند (صافي الدخل، التغير في الذمم المدينة، إلخ)
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// قيمة البند
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// مستوى المسافة البادئة (لتحقيق مظهر شجري)
        /// </summary>
        public int IndentLevel { get; set; }

        /// <summary>
        /// هل هذا البند هو إجمالي (لعرضه بخط عريض)؟
        /// </summary>
        public bool IsTotal { get; set; }
    }
}
