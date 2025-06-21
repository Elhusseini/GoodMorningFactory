// GoodMorningFactory/UI/ViewModels/PurchaseOrderItemViewModel.cs
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PurchaseOrderItemViewModel : INotifyPropertyChanged
    {
        // هذا يربط الـ ViewModel بنموذج البيانات مباشرة
        public PurchaseOrderItem Model { get; }

        // --- ملاحظة: هذه الخاصية لم تعد مستخدمة للربط في الواجهة، ولكنها قد تكون مفيدة داخلياً ---
        public List<Product> Products { get; }

        public int? ProductId
        {
            get => Model.ProductId == 0 ? null : (int?)Model.ProductId;
            set
            {
                if (value.HasValue && Model.ProductId != value.Value)
                {
                    Model.ProductId = value.Value;
                    OnPropertyChanged(nameof(ProductId));

                    // عند اختيار منتج، يتم تحديث السعر تلقائياً من قائمة المنتجات
                    var selectedProduct = Products.FirstOrDefault(p => p.Id == value.Value);
                    if (selectedProduct != null)
                    {
                        UnitPrice = selectedProduct.PurchasePrice;
                    }
                }
            }
        }

        public int Quantity
        {
            get => Model.Quantity;
            set
            {
                if (Model.Quantity != value)
                {
                    Model.Quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Subtotal)); // إشعار الواجهة بتغير الإجمالي الفرعي
                }
            }
        }

        public decimal UnitPrice
        {
            get => Model.UnitPrice;
            set
            {
                if (Model.UnitPrice != value)
                {
                    Model.UnitPrice = value;
                    OnPropertyChanged(nameof(UnitPrice));
                    OnPropertyChanged(nameof(Subtotal)); // إشعار الواجهة بتغير الإجمالي الفرعي
                }
            }
        }

        public decimal Subtotal => Quantity * UnitPrice;

        public PurchaseOrderItemViewModel(PurchaseOrderItem model, List<Product> products)
        {
            Model = model;
            Products = products;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}