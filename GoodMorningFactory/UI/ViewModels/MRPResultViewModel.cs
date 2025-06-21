// UI/ViewModels/MRPResultViewModel.cs
// يمثل هذا الكلاس نتيجة حساب تخطيط المتطلبات لمادة خام واحدة

namespace GoodMorningFactory.UI.ViewModels
{
    public class MRPResultViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitOfMeasure { get; set; }

        // إجمالي الكمية المطلوبة بناءً على أوامر البيع
        public decimal GrossRequirements { get; set; }

        // الكمية المتوفرة حالياً في المخزون
        public int OnHandInventory { get; set; }

        // الكمية المتوقع استلامها من أوامر الشراء المفتوحة
        public int ScheduledReceipts { get; set; }

        // صافي الاحتياج (الكمية المطلوب شراؤها)
        public decimal NetRequirements { get; set; }

        // خاصية محسوبة لتحديد ما إذا كان يمكن إنشاء طلب شراء
        // (فقط إذا كان صافي الاحتياج أكبر من صفر)
        public bool CanCreateRequisition => NetRequirements > 0;
    }
}
