// GoodMorningFactory/UI/ViewModels/CurrenciesViewModel.cs
// *** ملف جديد: ViewModel لواجهة عرض العملات ***
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
    public class CurrenciesViewModel : BaseViewModel
    {
        private readonly ICurrencyService _currencyService;

        public ObservableCollection<Currency> Currencies { get; } = new ObservableCollection<Currency>();

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public CurrenciesViewModel()
        {
            _currencyService = new CurrencyService();
            AddCommand = new RelayCommand(AddCurrency);
            EditCommand = new RelayCommand(EditCurrency, CanExecute);
            DeleteCommand = new RelayCommand(async (p) => await DeleteCurrencyAsync(p), CanExecute);
            RefreshCommand = new RelayCommand(async (p) => await LoadCurrenciesAsync());

            LoadCurrenciesAsync();
        }

        private async Task LoadCurrenciesAsync()
        {
            try
            {
                var currenciesList = await _currencyService.GetCurrenciesAsync();
                Currencies.Clear();
                foreach (var currency in currenciesList)
                {
                    Currencies.Add(currency);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل العملات: {ex.Message}", "خطأ");
            }
        }

        private void AddCurrency(object parameter)
        {
            var addWindow = new AddEditCurrencyWindow();
            if (addWindow.ShowDialog() == true) LoadCurrenciesAsync();
        }

        private void EditCurrency(object parameter)
        {
            if (parameter is Currency currency)
            {
                var editWindow = new AddEditCurrencyWindow(currency.Id);
                if (editWindow.ShowDialog() == true) LoadCurrenciesAsync();
            }
        }

        private async Task DeleteCurrencyAsync(object parameter)
        {
            if (parameter is Currency currency)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف العملة '{currency.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _currencyService.DeleteCurrencyAsync(currency.Id);
                        Currencies.Remove(currency);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل حذف العملة: {ex.Message}", "خطأ");
                    }
                }
            }
        }

        private bool CanExecute(object parameter) => parameter != null;
    }
}