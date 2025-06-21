// UI/ViewModels/GoodsReceiptItemViewModel.cs
// تم تعديل هذا الملف لإضافة خصائص لتخزين بيانات التتبع
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    // كلاس مساعد لتخزين معلومات الدفعة
    public class LotNumberInfo
    {
        public string Value { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class GoodsReceiptItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public int PreviouslyReceivedQuantity { get; set; }
        public int QuantityReceived { get; set; }
        public decimal UnitPrice { get; set; }
        public int? DestinationLocationId { get; set; }
        public List<StorageLocation> AvailableLocations { get; set; }
        public bool IsTracked { get; set; }
        public ProductTrackingMethod TrackingMethod { get; set; } // خاصية جديدة لتحديد نوع التتبع

        // --- بداية الإضافة: خصائص لتخزين بيانات التتبع ---
        public List<string> SerialNumbers { get; set; } = new List<string>();
        public LotNumberInfo LotInfo { get; set; }
        // --- نهاية الإضافة ---

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
