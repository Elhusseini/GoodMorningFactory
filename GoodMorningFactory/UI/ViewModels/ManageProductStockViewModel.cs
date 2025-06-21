// GoodMorningFactory/UI/ViewModels/ManageProductStockViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    // ViewModel مساعد لتمثيل رصيد المنتج في موقع معين
    public class ProductStockLocationViewModel : BaseViewModel
    {
        public int StorageLocationId { get; set; }
        public string StorageLocationName { get; set; }
        public int CurrentQuantity { get; set; }
        private int _newQuantity;
        public int NewQuantity { get => _newQuantity; set { _newQuantity = value; OnPropertyChanged(); } }
    }

    public class ManageProductStockViewModel : BaseViewModel
    {
        private readonly IInventoryService _inventoryService;
        private readonly ProductViewModel _product;

        public string WindowTitle { get; private set; }
        public ObservableCollection<ProductStockLocationViewModel> StockLocations { get; set; }
        public string Reason { get; set; }

        public RelayCommand SaveCommand { get; }

        public ManageProductStockViewModel(ProductViewModel product)
        {
            _inventoryService = new InventoryService();
            _product = product;
            WindowTitle = $"إدارة مخزون المنتج: {product.Name}";
            StockLocations = new ObservableCollection<ProductStockLocationViewModel>();

            SaveCommand = new RelayCommand(Save, CanSave);

            LoadStockLevels();
        }

        private async void LoadStockLevels()
        {
            try
            {
                var levels = await _inventoryService.GetStockLevelsForProductAsync(_product.Id);
                StockLocations.Clear();
                foreach (var level in levels)
                {
                    StockLocations.Add(level);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل أرصدة المخزون: {ex.Message}", "خطأ");
            }
        }

        private bool CanSave(object parameter)
        {
            return StockLocations.Any(l => l.NewQuantity != l.CurrentQuantity);
        }

        private async void Save(object parameter)
        {
            if (!(parameter is Window window)) return;

            try
            {
                await _inventoryService.UpdateStockLevelsForProductAsync(_product.Id, StockLocations, Reason);
                MessageBox.Show("تم تحديث أرصدة المخزون بنجاح.", "نجاح");
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ التغييرات: {ex.Message}", "خطأ");
            }
        }
    }
}