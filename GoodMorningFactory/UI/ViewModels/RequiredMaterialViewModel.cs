// UI/ViewModels/RequiredMaterialViewModel.cs
// *** ملف جديد: ViewModel لعرض بيانات المواد المطلوبة لأمر العمل ***
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class RequiredMaterialViewModel : INotifyPropertyChanged
    {
        public int MaterialId { get; set; } // تم تغيير الاسم ليكون أوضح
        public string MaterialName { get; set; }
        public decimal RequiredQuantity { get; set; }
        public decimal PreviouslyConsumedQuantity { get; set; }

        // خاصية محسوبة لعرض الكمية المتبقية للصرف
        public decimal RemainingToConsume => RequiredQuantity - PreviouslyConsumedQuantity;

        public int AvailableQuantity { get; set; }

        private decimal _consumedQuantity;
        public decimal ConsumedQuantity
        {
            get => _consumedQuantity;
            set
            {
                if (_consumedQuantity != value)
                {
                    _consumedQuantity = value;
                    OnPropertyChanged(nameof(ConsumedQuantity));
                }
            }
        }

        // خاصية لعرض حالة توفر المادة
        public string Status => AvailableQuantity >= (int)RequiredQuantity ? "متوفر" : "نقص";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}