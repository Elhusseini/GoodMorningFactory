using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GoodMorningFactory.UI.ViewModels
{
    public class StockTransferViewModel : BaseViewModel
    {
        private readonly IStockTransferService _transferService;

        #region Properties
        public string Notes { get; set; }
        public ObservableCollection<Warehouse> SourceWarehouses { get; set; }
        public ObservableCollection<Warehouse> DestinationWarehouses { get; set; }
        public ObservableCollection<StorageLocation> SourceLocations { get; set; }
        public ObservableCollection<StorageLocation> DestinationLocations { get; set; }

        private Warehouse _selectedSourceWarehouse;
        public Warehouse SelectedSourceWarehouse
        {
            get => _selectedSourceWarehouse;
            set { _selectedSourceWarehouse = value; OnPropertyChanged(); LoadSourceLocations(); }
        }

        private Warehouse _selectedDestinationWarehouse;
        public Warehouse SelectedDestinationWarehouse
        {
            get => _selectedDestinationWarehouse;
            set { _selectedDestinationWarehouse = value; OnPropertyChanged(); LoadDestinationLocations(); }
        }

        private StorageLocation _selectedSourceLocation;
        public StorageLocation SelectedSourceLocation
        {
            get => _selectedSourceLocation;
            set { _selectedSourceLocation = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSearchProduct)); ItemsToTransfer.Clear(); }
        }

        private StorageLocation _selectedDestinationLocation;
        public StorageLocation SelectedDestinationLocation { get => _selectedDestinationLocation; set { _selectedDestinationLocation = value; OnPropertyChanged(); } }

        public string ProductSearchText { get; set; }
        public bool CanSearchProduct => SelectedSourceLocation != null;

        public ObservableCollection<StockTransferItemViewModel> ItemsToTransfer { get; set; }
        #endregion

        #region Commands
        public ICommand SearchProductCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ExecuteTransferCommand { get; }
        #endregion

        public StockTransferViewModel()
        {
            _transferService = new StockTransferService();
            ItemsToTransfer = new ObservableCollection<StockTransferItemViewModel>();

            SearchProductCommand = new RelayCommand(SearchProduct);
            RemoveItemCommand = new RelayCommand(RemoveItem);
            ExecuteTransferCommand = new RelayCommand(ExecuteTransfer, CanExecuteTransfer);

            LoadInitialData();
        }

        // Constructor for design-time
        public StockTransferViewModel(bool forDesignTime)
        {
            // This constructor can be left empty or used to provide sample data for the designer
        }

        private async void LoadInitialData()
        {
            var initialData = await _transferService.GetInitialDataAsync();
            SourceWarehouses = new ObservableCollection<Warehouse>(initialData.Warehouses);
            DestinationWarehouses = new ObservableCollection<Warehouse>(initialData.Warehouses);
            OnPropertyChanged(nameof(SourceWarehouses));
            OnPropertyChanged(nameof(DestinationWarehouses));
        }

        private async void LoadSourceLocations()
        {
            SourceLocations?.Clear();
            OnPropertyChanged(nameof(SourceLocations));
            if (SelectedSourceWarehouse != null)
            {
                var locations = await _transferService.GetLocationsForWarehouseAsync(SelectedSourceWarehouse.Id);
                SourceLocations = new ObservableCollection<StorageLocation>(locations);
                OnPropertyChanged(nameof(SourceLocations));
            }
        }

        private async void LoadDestinationLocations()
        {
            DestinationLocations?.Clear();
            OnPropertyChanged(nameof(DestinationLocations));
            if (SelectedDestinationWarehouse != null)
            {
                var locations = await _transferService.GetLocationsForWarehouseAsync(SelectedDestinationWarehouse.Id);
                DestinationLocations = new ObservableCollection<StorageLocation>(locations);
                OnPropertyChanged(nameof(DestinationLocations));
            }
        }

        private async void SearchProduct(object parameter)
        {
            if (string.IsNullOrWhiteSpace(ProductSearchText)) return;

            var (product, availableQty) = await _transferService.FindProductForTransferAsync(ProductSearchText, SelectedSourceLocation.Id);
            if (product != null)
            {
                if (ItemsToTransfer.Any(i => i.ProductId == product.Id)) { MessageBox.Show("المنتج موجود بالفعل في القائمة."); return; }

                ItemsToTransfer.Add(new StockTransferItemViewModel
                {
                    ProductId = product.Id,
                    ProductCode = product.ProductCode,
                    ProductName = product.Name,
                    AvailableQuantity = availableQty,
                    QuantityToTransfer = 1
                });
                ProductSearchText = string.Empty; OnPropertyChanged(nameof(ProductSearchText));
            }
            else { MessageBox.Show("لم يتم العثور على المنتج في الموقع المحدد."); }
        }

        private void RemoveItem(object parameter)
        {
            if (parameter is StockTransferItemViewModel item) { ItemsToTransfer.Remove(item); }
        }

        private async void ExecuteTransfer(object parameter)
        {
            try
            {
                await _transferService.ExecuteTransferAsync(SelectedSourceLocation.Id, SelectedDestinationLocation.Id, Notes, ItemsToTransfer);
                MessageBox.Show("تم تنفيذ عملية التحويل بنجاح.", "نجاح");
                if (parameter is Window window) { window.DialogResult = true; window.Close(); }
            }
            catch (DbUpdateException dbEx)
            {
                var sb = new StringBuilder();
                sb.AppendLine("فشل تحديث قاعدة البيانات. التفاصيل:");
                sb.AppendLine(dbEx.ToString());
                if (dbEx.InnerException != null) { sb.AppendLine("\n--- الخطأ الداخلي ---\n" + dbEx.InnerException.ToString()); }
                MessageBox.Show(sb.ToString(), "خطأ قاعدة بيانات تفصيلي");
            }
            catch (Exception ex) { MessageBox.Show($"فشلت عملية التحويل: {ex.Message}", "خطأ فادح"); }
        }

        private bool CanExecuteTransfer(object parameter)
        {
            if (SelectedSourceLocation == null || SelectedDestinationLocation == null) return false;
            if (SelectedSourceLocation.Id == SelectedDestinationLocation.Id) return false;
            if (!ItemsToTransfer.Any(i => i.QuantityToTransfer > 0)) return false;
            if (ItemsToTransfer.Any(i => i.QuantityToTransfer > i.AvailableQuantity)) return false;
            return true;
        }
    }
}