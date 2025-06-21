// GoodMorningFactory/UI/ViewModels/AdjustStockViewModel.cs
// *** الكود الكامل والصحيح ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AdjustStockViewModel : BaseViewModel
    {
        private readonly IInventoryService _inventoryService;

        #region Properties
        public ObservableCollection<Warehouse> Warehouses { get; } = new ObservableCollection<Warehouse>();
        public ObservableCollection<StorageLocation> Locations { get; } = new ObservableCollection<StorageLocation>();
        public ObservableCollection<StockAdjustmentItemViewModel> ItemsToAdjust { get; } = new ObservableCollection<StockAdjustmentItemViewModel>();

        private Warehouse _selectedWarehouse;
        public Warehouse SelectedWarehouse { get => _selectedWarehouse; set { _selectedWarehouse = value; OnPropertyChanged(); LoadLocationsAsync(); } }

        private StorageLocation _selectedLocation;
        public StorageLocation SelectedLocation { get => _selectedLocation; set { _selectedLocation = value; OnPropertyChanged(); ClearItems(); } }

        private DateTime _adjustmentDate = DateTime.Today;
        public DateTime AdjustmentDate { get => _adjustmentDate; set { _adjustmentDate = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        #endregion

        public ICommand SearchProductCommand { get; }
        public ICommand PostAdjustmentCommand { get; }

        public AdjustStockViewModel()
        {
            _inventoryService = new InventoryService();
            SearchProductCommand = new RelayCommand(async (p) => await SearchAndAddItemAsync(), (p) => !string.IsNullOrWhiteSpace(SearchText) && SelectedLocation != null);
            PostAdjustmentCommand = new RelayCommand(async (p) => await PostAdjustmentAsync(p as Window), (p) => SelectedLocation != null && ItemsToAdjust.Any(i => i.Difference != 0));

            LoadWarehousesAsync();
        }

        private async void LoadWarehousesAsync()
        {
            IsLoading = true;
            try
            {
                var warehousesList = await _inventoryService.GetActiveWarehousesAsync();
                Warehouses.Clear();
                foreach (var wh in warehousesList) Warehouses.Add(wh);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
            finally { IsLoading = false; }
        }

        private async void LoadLocationsAsync()
        {
            Locations.Clear();
            ClearItems();
            if (SelectedWarehouse == null) return;

            IsLoading = true;
            try
            {
                var locationsList = await _inventoryService.GetActiveLocationsByWarehouseAsync(SelectedWarehouse.Id);
                foreach (var loc in locationsList) Locations.Add(loc);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
            finally { IsLoading = false; }
        }

        private async Task SearchAndAddItemAsync()
        {
            // الإصلاح هنا: استخدام ProductCode
            if (ItemsToAdjust.Any(i => i.ProductCode.Equals(SearchText, StringComparison.OrdinalIgnoreCase)))
            {
                SearchText = string.Empty;
                return;
            }

            try
            {
                var itemVm = await _inventoryService.GetProductForAdjustmentAsync(SearchText, SelectedLocation.Id);
                if (itemVm != null)
                {
                    ItemsToAdjust.Add(itemVm);
                }
                else
                {
                    MessageBox.Show("لم يتم العثور على المنتج.", "بحث", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
            finally { SearchText = string.Empty; }
        }

        private async Task PostAdjustmentAsync(Window window)
        {
            IsLoading = true;
            try
            {
                var adjustment = new StockAdjustment
                {
                    ReferenceNumber = $"ADJ-{DateTime.Now:yyyyMMddHHmmss}",
                    AdjustmentDate = AdjustmentDate,
                    WarehouseId = SelectedWarehouse.Id,
                    StorageLocationId = SelectedLocation.Id, // الإصلاح هنا
                    Reason = "جرد وتعديل مخزون",
                    StockAdjustmentItems = new Collection<StockAdjustmentItem>(
                        ItemsToAdjust.Where(i => i.Difference != 0)
                                     .Select(i => new StockAdjustmentItem
                                     {
                                         ProductId = i.ProductId,
                                         SystemQuantity = i.SystemQuantity,
                                         CountedQuantity = i.ActualQuantity,
                                         // ======================= بداية الإصلاح =======================
                                         UnitCost = i.UnitCost // تمرير التكلفة هنا
                                         // ======================== نهاية الإصلاح ========================
                                     }).ToList())
                };

                await _inventoryService.PostStockAdjustmentAsync(adjustment);
                MessageBox.Show("تم ترحيل تعديلات المخزون بنجاح.", "نجاح");
                window.DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private void ClearItems()
        {
            if (ItemsToAdjust.Any()) ItemsToAdjust.Clear();
        }
    }
}