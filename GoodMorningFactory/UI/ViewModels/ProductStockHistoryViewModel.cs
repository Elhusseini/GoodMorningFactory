// GoodMorningFactory/UI/ViewModels/ProductStockHistoryViewModel.cs
// *** ملف جديد: ViewModel لنافذة سجل حركات المنتج ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ProductStockHistoryViewModel : BaseViewModel
    {
        private readonly IInventoryService _inventoryService;
        private readonly int _productId;

        private string _productNameText;
        public string ProductNameText
        {
            get => _productNameText;
            set { _productNameText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<StockMovementViewModel> History { get; } = new ObservableCollection<StockMovementViewModel>();

        public ProductStockHistoryViewModel(int productId)
        {
            _productId = productId;
            _inventoryService = new InventoryService();
            LoadHistoryAsync();
        }

        private async void LoadHistoryAsync()
        {
            try
            {
                using (var db = new Data.DatabaseContext())
                {
                    var product = await db.Products.FindAsync(_productId);
                    if (product == null) return;
                    ProductNameText = $"سجل حركات المنتج: {product.Name}";
                }

                var movements = await _inventoryService.GetStockMovementsForProductAsync(_productId);
                History.Clear();
                foreach (var movement in movements)
                {
                    History.Add(movement);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل سجل الحركات: {ex.Message}", "خطأ");
            }
        }
    }
}