// GoodMorningFactory/UI/ViewModels/SerialNumbersViewModel.cs
// *** ملف جديد: ViewModel لواجهة عرض الأرقام التسلسلية ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SerialNumbersViewModel : BaseViewModel
    {
        private readonly IInventoryService _inventoryService;

        #region Properties
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SerialNumber> SerialNumbers { get; } = new ObservableCollection<SerialNumber>();
        #endregion

        #region Commands
        public ICommand SearchCommand { get; }
        #endregion

        public SerialNumbersViewModel()
        {
            _inventoryService = new InventoryService();
            SearchCommand = new RelayCommand(async (p) => await LoadSerialNumbersAsync());
            LoadSerialNumbersAsync();
        }

        private async Task LoadSerialNumbersAsync()
        {
            try
            {
                var serials = await _inventoryService.GetSerialNumbersAsync(SearchText);
                SerialNumbers.Clear();
                foreach (var serial in serials)
                {
                    SerialNumbers.Add(serial);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الأرقام التسلسلية: {ex.Message}", "خطأ");
            }
        }
    }
}