using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel مخصص لعرض بيانات العميل في الواجهة.
    /// يرث الآن من BaseViewModel للاتساق مع بنية المشروع.
    /// </summary>
    public class CustomerViewModel : BaseViewModel
    {
        // الخصائص التالية تمثل بيانات العميل الأساسية
        public int Id { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public bool IsActive { get; set; }

        // خاصية إضافية لحساب وعرض الرصيد الحالي للعميل
        public decimal CurrentBalance { get; set; }
    }
}
