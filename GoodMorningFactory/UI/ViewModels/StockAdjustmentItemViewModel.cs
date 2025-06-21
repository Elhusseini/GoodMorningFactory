// UI/ViewModels/StockAdjustmentItemViewModel.cs
// *** تحديث: تمت إضافة خصائص منسقة للعملة وإصلاح آلية التحديث ***
using GoodMorningFactory.Core.Services;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class StockAdjustmentItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int SystemQuantity { get; set; }

        private int _actualQuantity;
        public int ActualQuantity
        {
            get => _actualQuantity;
            set
            {
                if (_actualQuantity != value)
                {
                    _actualQuantity = value;
                    OnPropertyChanged(nameof(ActualQuantity));
                    OnPropertyChanged(nameof(Difference));
                    OnPropertyChanged(nameof(DifferenceValue));
                    OnPropertyChanged(nameof(DifferenceValueFormatted)); // تحديث القيمة المنسقة
                }
            }
        }

        public int Difference => ActualQuantity - SystemQuantity;

        public decimal UnitCost { get; set; }
        public decimal DifferenceValue => Difference * UnitCost;

        // --- بداية التحديث: خصائص منسقة للعملة ---
        public string UnitCostFormatted => $"{UnitCost:N2} {AppSettings.DefaultCurrencySymbol}";
        public string DifferenceValueFormatted => $"{DifferenceValue:N2} {AppSettings.DefaultCurrencySymbol}";
        // --- نهاية التحديث ---

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
