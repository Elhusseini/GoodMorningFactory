namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل بنداً واحداً (منتجاً) في نافذة إنشاء مرتجع المبيعات.
    /// يرث من BaseViewModel ليدعم إشعارات تغيير الخصائص للواجهة.
    /// </summary>
    public class SalesReturnItemViewModel : BaseViewModel
    {
        // معرف المنتج
        public int ProductId { get; set; }

        // اسم المنتج للعرض
        public string ProductName { get; set; }

        // الكمية الأصلية التي تم بيعها في الفاتورة
        public int OriginalQuantity { get; set; }

        // الكمية التي تم إرجاعها من هذا المنتج في مرتجعات سابقة لنفس الفاتورة
        public int PreviouslyReturnedQuantity { get; set; }

        // الكمية الحالية التي يرغب المستخدم في إرجاعها (هذه الخاصية مرتبطة بالواجهة)
        private int _quantityToReturn;
        public int QuantityToReturn
        {
            get => _quantityToReturn;
            set
            {
                _quantityToReturn = value;
                OnPropertyChanged(); // إشعار الواجهة بتغير القيمة
            }
        }

        // سعر الوحدة للمنتج كما تم بيعه في الفاتورة الأصلية (لحساب قيمة المرتجع)
        public decimal UnitPrice { get; set; }
    }
}
