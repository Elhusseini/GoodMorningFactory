namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل بندًا واحدًا في فاتورة البيع المباشر.
    /// </summary>
    public class DirectSaleItemViewModel : BaseViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); }
        }

        public decimal Subtotal => UnitPrice * Quantity;
    }
}
