// GoodMorningFactory/UI/ViewModels/AddEditStorageLocationViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditStorageLocationViewModel : BaseViewModel
    {
        private readonly IWarehouseService _warehouseService;
        private StorageLocation _location;
        private readonly int _warehouseId;

        #region Properties
        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _code;
        public string Code { get => _code; set { _code = value; OnPropertyChanged(); } }

        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private bool _isActive;
        public bool IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged(); } }

        private bool _isDefault;
        public bool IsDefault { get => _isDefault; set { _isDefault = value; OnPropertyChanged(); } }
        #endregion

        public ICommand SaveCommand { get; }

        // --- بداية الإضافة: مُنشئ فارغ لوقت التصميم ---
        /// <summary>
        /// هذا المُنشئ الفارغ يُستخدم فقط بواسطة مصمم الواجهات (XAML Designer)
        /// لمنع ظهور الأخطاء في Visual Studio.
        /// </summary>
        public AddEditStorageLocationViewModel()
        {
            // يمكن ترك هذا فارغاً أو إضافة بيانات وهمية للعرض في المصمم
            Title = "إضافة / تعديل موقع";
        }
        // --- نهاية الإضافة ---

        public AddEditStorageLocationViewModel(int warehouseId, int? locationId = null)
        {
            _warehouseService = new WarehouseService();
            _warehouseId = warehouseId;
            SaveCommand = new RelayCommand(Save, CanSave);

            if (locationId.HasValue)
            {
                Title = "تعديل موقع تخزين";
                LoadLocation(locationId.Value);
            }
            else
            {
                Title = "إضافة موقع جديد";
                _location = new StorageLocation { WarehouseId = _warehouseId, IsActive = true };
                IsActive = true;
            }
        }

        private async void LoadLocation(int id)
        {
            _location = await _warehouseService.GetLocationByIdAsync(id);
            if (_location != null)
            {
                Code = _location.Code;
                Name = _location.Name;
                Description = _location.Description;
                IsActive = _location.IsActive;
                IsDefault = _location.IsDefault;
                OnPropertyChanged(string.Empty);
            }
        }

        private async void Save(object parameter)
        {
            _location.Code = this.Code ?? string.Empty;
            _location.Name = this.Name ?? string.Empty;
            _location.Description = this.Description ?? string.Empty;
            _location.IsActive = this.IsActive;
            _location.IsDefault = this.IsDefault;

            try
            {
                await _warehouseService.SaveLocationAsync(_location);
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الموقع: {ex.Message}", "خطأ");
            }
        }

        private bool CanSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(Name);
        }
    }
}