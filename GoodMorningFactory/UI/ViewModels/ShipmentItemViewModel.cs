// UI/ViewModels/ShipmentItemViewModel.cs
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ShipmentItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public int PreviouslyShippedQuantity { get; set; }
        public int QuantityToShip { get; set; }
        public decimal UnitPrice { get; set; }
        public int? SourceLocationId { get; set; }
        public List<AvailableStockLocation> AvailableLocations { get; set; }
        public bool IsTracked { get; set; }
        public ProductTrackingMethod TrackingMethod { get; set; }

        // --- بداية الإضافة: خاصية لتخزين الأرقام التسلسلية المختارة ---
        public List<long> SelectedSerialIds { get; set; } = new List<long>();
        // --- نهاية الإضافة ---

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
