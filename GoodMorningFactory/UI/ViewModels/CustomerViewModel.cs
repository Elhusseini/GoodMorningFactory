// UI/ViewModels/CustomerViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض بيانات العملاء مع أرصدتهم ***
using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory.UI.ViewModels
{
    // هذا الكلاس يرث من كلاس العميل الأصلي ويضيف إليه خاصية الرصيد المحسوب
    public class CustomerViewModel : Customer
    {
        // خاصية إضافية لحساب وعرض الرصيد الحالي للعميل (المبالغ المستحقة عليه)
        public decimal CurrentBalance { get; set; }
    }
}
