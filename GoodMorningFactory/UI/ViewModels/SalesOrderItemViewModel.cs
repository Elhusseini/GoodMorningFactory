using GoodMorningFactory.Core.Services;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SalesOrderItemViewModel : BaseViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set { _unitPrice = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(SubtotalFormatted)); }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(SubtotalFormatted)); }
        }

        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set { _discount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(SubtotalFormatted)); }
        }

        public decimal Subtotal => (UnitPrice * Quantity) - Discount;
        public string SubtotalFormatted => $"{Subtotal:N2} {AppSettings.DefaultCurrencySymbol}";
    }
}
