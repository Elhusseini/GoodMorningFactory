using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class WarehousesViewModel : BaseViewModel
    {
        private readonly IWarehouseService _warehouseService;

        #region Properties
        private ObservableCollection<Warehouse> _warehouses;
        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set { _warehouses = value; OnPropertyChanged(); }
        }

        private Warehouse _selectedWarehouse;
        public Warehouse SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                _selectedWarehouse = value;
                OnPropertyChanged();
                LoadLocationsForSelectedWarehouse();
            }
        }

        private ObservableCollection<StorageLocation> _storageLocations;
        public ObservableCollection<StorageLocation> StorageLocations
        {
            get => _storageLocations;
            set { _storageLocations = value; OnPropertyChanged(); }
        }

        public string LocationsGroupBoxHeader => SelectedWarehouse != null ? $"المواقع الفرعية في: {SelectedWarehouse.Name}" : "المواقع الفرعية";
        public bool IsLocationsGroupBoxEnabled => SelectedWarehouse != null;
        #endregion

        #region Commands
        public ICommand AddWarehouseCommand { get; }
        public ICommand EditWarehouseCommand { get; }
        public ICommand AddLocationCommand { get; }
        public ICommand EditLocationCommand { get; }
        public ICommand DeleteLocationCommand { get; }
        #endregion

        public WarehousesViewModel()
        {
            _warehouseService = new WarehouseService();
            Warehouses = new ObservableCollection<Warehouse>();
            StorageLocations = new ObservableCollection<StorageLocation>();

            AddWarehouseCommand = new RelayCommand(ExecuteAddWarehouse);
            EditWarehouseCommand = new RelayCommand(ExecuteEditWarehouse, CanExecuteOnWarehouse);
            AddLocationCommand = new RelayCommand(ExecuteAddLocation, CanExecuteOnWarehouse);
            EditLocationCommand = new RelayCommand(ExecuteEditLocation, CanExecuteOnLocation);
            DeleteLocationCommand = new RelayCommand(ExecuteDeleteLocation, CanExecuteOnLocation);

            LoadWarehousesAsync();
        }

        private async void LoadWarehousesAsync()
        {
            try
            {
                var warehouses = await _warehouseService.GetWarehousesAsync();
                Warehouses = new ObservableCollection<Warehouse>(warehouses);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل المخازن: {ex.Message}", "خطأ");
            }
        }

        private async void LoadLocationsForSelectedWarehouse()
        {
            OnPropertyChanged(nameof(LocationsGroupBoxHeader));
            OnPropertyChanged(nameof(IsLocationsGroupBoxEnabled));
            StorageLocations.Clear();

            if (SelectedWarehouse == null) return;

            try
            {
                var locations = await _warehouseService.GetLocationsForWarehouseAsync(SelectedWarehouse.Id);
                StorageLocations = new ObservableCollection<StorageLocation>(locations);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل المواقع: {ex.Message}", "خطأ");
            }
        }

        // Warehouse Commands
        private void ExecuteAddWarehouse(object obj)
        {
            var addWindow = new AddEditWarehouseWindow();
            if (addWindow.ShowDialog() == true) LoadWarehousesAsync();
        }

        private void ExecuteEditWarehouse(object parameter)
        {
            if (parameter is Warehouse warehouse)
            {
                var editWindow = new AddEditWarehouseWindow(warehouse.Id);
                if (editWindow.ShowDialog() == true) LoadWarehousesAsync();
            }
        }

        // Location Commands
        private void ExecuteAddLocation(object obj)
        {
            var addWindow = new AddEditStorageLocationWindow(SelectedWarehouse.Id);
            if (addWindow.ShowDialog() == true) LoadLocationsForSelectedWarehouse();
        }

        private void ExecuteEditLocation(object parameter)
        {
            if (parameter is StorageLocation location)
            {
                var editWindow = new AddEditStorageLocationWindow(location.WarehouseId, location.Id);
                if (editWindow.ShowDialog() == true) LoadLocationsForSelectedWarehouse();
            }
        }

        private async void ExecuteDeleteLocation(object parameter)
        {
            if (parameter is StorageLocation location)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف الموقع '{location.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _warehouseService.DeleteLocationAsync(location.Id);
                        LoadLocationsForSelectedWarehouse();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل حذف الموقع: {ex.Message}", "خطأ");
                    }
                }
            }
        }

        private bool CanExecuteOnWarehouse(object parameter) => SelectedWarehouse != null;
        private bool CanExecuteOnLocation(object parameter) => parameter is StorageLocation;
    }
}