// GoodMorning/UI/ViewModels/InventoryCountItemViewModel.cs
// *** ملف جديد: ViewModel لتمثيل سطر واحد في نافذة تفاصيل الجرد ***
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class InventoryCountItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int StorageLocationId { get; set; }
        public string StorageLocationName { get; set; }
        public int SystemQuantity { get; set; }

        private int _countedQuantity;
        public int CountedQuantity
        {
            get => _countedQuantity;
            set
            {
                if (_countedQuantity != value)
                {
                    _countedQuantity = value;
                    OnPropertyChanged(nameof(CountedQuantity));
                    OnPropertyChanged(nameof(Difference)); // تحديث الفرق تلقائياً
                }
            }
        }

        public int Difference => CountedQuantity - SystemQuantity;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
