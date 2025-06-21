namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل بندًا واحدًا (منتجًا) يمكن فوترته في نافذة إنشاء الفاتورة.
    /// </summary>
    public class InvoiceItemViewModel : BaseViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OrderedQuantity { get; set; } // الكمية المطلوبة في أمر البيع
        public int InvoicedQuantity { get; set; } // الكمية التي تمت فوترتها سابقًا

        private int _quantityToInvoice;
        public int QuantityToInvoice // الكمية المراد فوترتها في هذه الفاتورة
        {
            get => _quantityToInvoice;
            set { _quantityToInvoice = value; OnPropertyChanged(); }
        }

        public decimal UnitPrice { get; set; }
        public int MaxQuantityToInvoice => OrderedQuantity - InvoicedQuantity; // أقصى كمية يمكن فوترتها
    }
}
