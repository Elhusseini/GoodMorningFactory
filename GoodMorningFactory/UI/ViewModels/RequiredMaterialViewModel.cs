// UI/ViewModels/RequiredMaterialViewModel.cs
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GoodMorningFactory.UI.ViewModels
{
    // هذا النموذج يمثل موقع تخزين متاح للمادة الخام
    public class AvailableStockLocation
    {
        public int StorageLocationId { get; set; }
        public string LocationName { get; set; }
        public int QuantityOnHand { get; set; }
        public string DisplayName => $"{LocationName} (المتاح: {QuantityOnHand})";
    }

    public class RequiredMaterialViewModel : INotifyPropertyChanged
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public decimal RequiredQuantity { get; set; }
        public decimal PreviouslyConsumedQuantity { get; set; }

        public decimal RemainingToConsume => RequiredQuantity - PreviouslyConsumedQuantity;

        public List<AvailableStockLocation> AvailableLocations { get; set; }

        private int? _sourceLocationId;
        public int? SourceLocationId
        {
            get => _sourceLocationId;
            set
            {
                if (_sourceLocationId != value)
                {
                    _sourceLocationId = value;
                    OnPropertyChanged(nameof(SourceLocationId));
                }
            }
        }

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

        // --- بداية الإضافة: خصائص لدعم التتبع ---
        public bool IsTracked { get; set; }
        public ProductTrackingMethod TrackingMethod { get; set; }
        public List<long> SelectedSerialIds { get; set; } = new List<long>();
        // --- نهاية الإضافة ---

        public string Status
        {
            get
            {
                int totalAvailable = AvailableLocations?.Sum(l => l.QuantityOnHand) ?? 0;
                return totalAvailable >= (int)RequiredQuantity ? "متوفر" : "نقص";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
