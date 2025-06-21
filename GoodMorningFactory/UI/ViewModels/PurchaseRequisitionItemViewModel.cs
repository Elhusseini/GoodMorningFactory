// GoodMorningFactory/UI/ViewModels/PurchaseRequisitionItemViewModel.cs
// *** الكود الكامل والمعدل - أصبح أكثر ذكاءً ***
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PurchaseRequisitionItemViewModel : BaseViewModel
    {
        private readonly List<Product> _allProducts;
        public PurchaseRequisitionItem Model { get; }

        private int? _productId;
        public int? ProductId
        {
            get => _productId;
            set
            {
                if (_productId != value)
                {
                    _productId = value;
                    Model.ProductId = value;
                    UpdateProductDetails();
                    OnPropertyChanged();
                }
            }
        }

        private Product _product;
        public Product Product
        {
            get => _product;
            private set { _product = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => Model.Description;
            set { if (Model.Description != value) { Model.Description = value; OnPropertyChanged(); } }
        }

        public decimal Quantity
        {
            get => Model.Quantity;
            set { if (Model.Quantity != value) { Model.Quantity = value; OnPropertyChanged(); } }
        }

        public string UnitOfMeasure
        {
            get => Model.UnitOfMeasure;
            set { if (Model.UnitOfMeasure != value) { Model.UnitOfMeasure = value; OnPropertyChanged(); } }
        }

        public PurchaseRequisitionItemViewModel(PurchaseRequisitionItem item, List<Product> allProducts)
        {
            Model = item;
            _allProducts = allProducts;
            _productId = item.ProductId;

            // تحديث بيانات المنتج عند تحميل بند موجود مسبقًا
            if (item.ProductId.HasValue)
            {
                UpdateProductDetails();
            }
        }

        private void UpdateProductDetails()
        {
            if (ProductId.HasValue && _allProducts != null)
            {
                var selectedProduct = _allProducts.FirstOrDefault(p => p.Id == ProductId.Value);
                if (selectedProduct != null)
                {
                    this.Product = selectedProduct;
                    this.Description = selectedProduct.Name;
                    this.UnitOfMeasure = selectedProduct.UnitOfMeasure?.Name ?? string.Empty;
                }
            }
        }
    }
}