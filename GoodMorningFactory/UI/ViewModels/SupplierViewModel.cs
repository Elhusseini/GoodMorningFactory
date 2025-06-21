// UI/ViewModels/SupplierViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض بيانات الموردين مع أرصدتهم ***
using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory.UI.ViewModels
{
    // هذا الكلاس يرث من كلاس المورد الأصلي ويضيف إليه خاصية الرصيد المحسوب
    public class SupplierViewModel : Supplier
    {
        // خاصية إضافية لحساب وعرض الرصيد الحالي للمورد (المبالغ المستحقة له)
        public decimal CurrentBalance { get; set; }
    }
}