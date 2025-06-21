// GoodMorningFactory/UI/ViewModels/EditSaleItemViewModel.cs
// *** ملف جديد: ViewModel لبند واحد في فاتورة التعديل ***
using GoodMorningFactory.Core.Services; // For AppSettings
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class EditSaleItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set { if (_unitPrice != value) { _unitPrice = value; OnAllPropertiesChanged(); } }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { if (_quantity != value) { _quantity = value; OnAllPropertiesChanged(); } }
        }

        public decimal Subtotal => UnitPrice * Quantity;
        public string SubtotalFormatted => $"{Subtotal:N2} {AppSettings.DefaultCurrencySymbol}";

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnAllPropertiesChanged()
        {
            OnPropertyChanged(nameof(UnitPrice));
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(SubtotalFormatted));
        }
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}