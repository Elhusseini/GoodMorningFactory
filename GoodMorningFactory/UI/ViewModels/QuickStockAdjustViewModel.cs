// GoodMorningFactory/UI/ViewModels/QuickStockAdjustViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class QuickStockAdjustViewModel : BaseViewModel
    {
        private readonly IInventoryService _inventoryService;

        #region Properties
        public string ProductName { get; private set; }
        public string StorageLocationName { get; private set; }
        public int SystemQuantity { get; private set; }

        private int _newQuantity;
        public int NewQuantity { get => _newQuantity; set { _newQuantity = value; OnPropertyChanged(); } }

        private string _reason;
        public string Reason { get => _reason; set { _reason = value; OnPropertyChanged(); } }
        #endregion

        public RelayCommand SaveCommand { get; }

        private readonly int _productId;
        private readonly int _storageLocationId;

        // مُنشئ فارغ لوقت التصميم
        public QuickStockAdjustViewModel() { }

        public QuickStockAdjustViewModel(int productId, int storageLocationId)
        {
            _inventoryService = new InventoryService();
            _productId = productId;
            _storageLocationId = storageLocationId;

            SaveCommand = new RelayCommand(Save, CanSave);
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                var data = await _inventoryService.GetDataForQuickAdjustmentAsync(_productId, _storageLocationId);
                ProductName = data.ProductName;
                StorageLocationName = data.StorageLocationName;
                SystemQuantity = data.SystemQuantity;
                NewQuantity = data.SystemQuantity;

                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(StorageLocationName));
                OnPropertyChanged(nameof(SystemQuantity));
                OnPropertyChanged(nameof(NewQuantity));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private bool CanSave(object parameter)
        {
            return NewQuantity != SystemQuantity;
        }

        private async void Save(object parameter)
        {
            if (!(parameter is Window window)) return;

            try
            {
                await _inventoryService.PerformQuickAdjustmentAsync(_productId, _storageLocationId, NewQuantity, Reason);
                MessageBox.Show("تم تحديث رصيد المخزون والقيد المحاسبي بنجاح.", "نجاح");
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ التعديل: {ex.Message}", "خطأ فادح");
            }
        }
    }
}