using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditWarehouseViewModel : BaseViewModel
    {
        private readonly IWarehouseService _warehouseService;
        private Warehouse _warehouse;

        #region Properties
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        #endregion

        public ICommand SaveCommand { get; }

        public AddEditWarehouseViewModel(int? warehouseId = null)
        {
            _warehouseService = new WarehouseService();
            SaveCommand = new RelayCommand(Save, CanSave);

            if (warehouseId.HasValue)
            {
                LoadWarehouse(warehouseId.Value);
            }
            else
            {
                _warehouse = new Warehouse { IsActive = true };
            }
        }

        private async void LoadWarehouse(int id)
        {
            _warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
            if (_warehouse != null)
            {
                Code = _warehouse.Code;
                Name = _warehouse.Name;
                Address = _warehouse.Address;
                OnPropertyChanged(string.Empty);
            }
        }

        private async void Save(object parameter)
        {
            _warehouse.Code = Code;
            _warehouse.Name = Name;
            _warehouse.Address = Address;

            try
            {
                await _warehouseService.SaveWarehouseAsync(_warehouse);
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ المخزن: {ex.Message}", "خطأ");
            }
        }

        private bool CanSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(Name);
        }
    }
}