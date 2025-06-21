// GoodMorningFactory/UI/ViewModels/PurchaseReturnItemViewModel.cs
using GoodMorningFactory.Data.Models;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PurchaseReturnItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OriginalQuantity { get; set; }
        public decimal UnitPrice { get; set; }

        private int _quantityToReturn;
        public int QuantityToReturn
        {
            get => _quantityToReturn;
            set
            {
                if (_quantityToReturn != value)
                {
                    _quantityToReturn = value;
                    OnPropertyChanged(nameof(QuantityToReturn));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}