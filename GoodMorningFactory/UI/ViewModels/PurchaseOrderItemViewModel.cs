// UI/ViewModels/PurchaseOrderItemViewModel.cs
// *** تحديث: تمت إضافة حقل الوصف ***
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PurchaseOrderItemViewModel : INotifyPropertyChanged
    {
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Description { get; set; } // <-- إضافة جديدة

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}