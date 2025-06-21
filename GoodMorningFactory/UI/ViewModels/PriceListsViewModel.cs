// GoodMorningFactory/UI/ViewModels/PriceListsViewModel.cs
// *** الكود الكامل لـ ViewModel الواجهة الرئيسية ***
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
    public class PriceListsViewModel : BaseViewModel
    {
        private readonly IPriceListService _priceListService;
        public ObservableCollection<PriceList> PriceLists { get; } = new ObservableCollection<PriceList>();

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ManagePricesCommand { get; }

        public PriceListsViewModel()
        {
            _priceListService = new PriceListService();
            AddCommand = new RelayCommand(AddPriceList);
            EditCommand = new RelayCommand(EditPriceList, CanExecute);
            DeleteCommand = new RelayCommand(async (p) => await DeletePriceListAsync(p), CanExecute);
            ManagePricesCommand = new RelayCommand(ManagePrices, CanExecute);

            LoadDataAsync();
        }

        private bool CanExecute(object parameter) => parameter != null;

        private async void LoadDataAsync()
        {
            try
            {
                var lists = await _priceListService.GetPriceListsAsync();
                PriceLists.Clear();
                foreach (var list in lists) PriceLists.Add(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}");
            }
        }

        private void AddPriceList(object parameter)
        {
            var addWindow = new AddEditPriceListWindow();
            if (addWindow.ShowDialog() == true) LoadDataAsync();
        }

        private void EditPriceList(object parameter)
        {
            if (parameter is PriceList priceList)
            {
                var editWindow = new AddEditPriceListWindow(priceList.Id);
                if (editWindow.ShowDialog() == true) LoadDataAsync();
            }
        }

        private async Task DeletePriceListAsync(object parameter)
        {
            if (parameter is PriceList priceList)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف القائمة '{priceList.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await _priceListService.DeletePriceListAsync(priceList.Id);
                    PriceLists.Remove(priceList);
                }
            }
        }

        private void ManagePrices(object parameter)
        {
            if (parameter is PriceList priceList)
            {
                var manageWindow = new ManageProductPricesWindow(priceList.Id);
                manageWindow.ShowDialog();
            }
        }
    }
}