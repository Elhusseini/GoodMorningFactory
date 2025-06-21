// UI/ViewModels/StockAdjustmentItemViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض بنود تعديل المخزون ***
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class StockAdjustmentItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int SystemQuantity { get; set; } // الكمية المسجلة في النظام

        private int _actualQuantity;
        public int ActualQuantity // الكمية الفعلية التي يدخلها المستخدم
        {
            get => _actualQuantity;
            set
            {
                if (_actualQuantity != value)
                {
                    _actualQuantity = value;
                    OnPropertyChanged(nameof(ActualQuantity));
                    OnPropertyChanged(nameof(Difference)); // تحديث الفرق تلقائياً
                }
            }
        }

        public int Difference => ActualQuantity - SystemQuantity; // الفرق بين الفعلي والمسجل

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}