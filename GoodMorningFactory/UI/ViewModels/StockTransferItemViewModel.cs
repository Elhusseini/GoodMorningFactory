// UI/ViewModels/StockTransferItemViewModel.cs
// *** ملف جديد: ViewModel لتمثيل سطر واحد في نافذة التحويل ***
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class StockTransferItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int AvailableQuantity { get; set; }

        private int _quantityToTransfer;
        public int QuantityToTransfer
        {
            get => _quantityToTransfer;
            set
            {
                if (_quantityToTransfer != value)
                {
                    _quantityToTransfer = value;
                    OnPropertyChanged(nameof(QuantityToTransfer));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
