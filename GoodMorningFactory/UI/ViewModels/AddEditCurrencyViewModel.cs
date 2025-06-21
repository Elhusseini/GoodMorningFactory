// GoodMorningFactory/UI/ViewModels/AddEditCurrencyViewModel.cs
// *** ملف جديد: ViewModel لنافذة إضافة وتعديل العملات ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditCurrencyViewModel : BaseViewModel
    {
        private readonly ICurrencyService _currencyService;

        private Currency _currency;
        public Currency Currency
        {
            get => _currency;
            set { _currency = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; }
        public RelayCommand SaveCommand { get; }

        public AddEditCurrencyViewModel(int? currencyId)
        {
            _currencyService = new CurrencyService();
            SaveCommand = new RelayCommand(async (p) => await SaveAsync(p as Window));
            LoadDataAsync(currencyId);
        }

        private async void LoadDataAsync(int? currencyId)
        {
            if (currencyId.HasValue)
            {
                WindowTitle = "تعديل عملة";
                Currency = await _currencyService.GetCurrencyByIdAsync(currencyId.Value);
            }
            else
            {
                WindowTitle = "إضافة عملة جديدة";
                Currency = new Currency { IsActive = true };
            }
            OnPropertyChanged(nameof(WindowTitle));
        }

        private async Task SaveAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(Currency.Name) || string.IsNullOrWhiteSpace(Currency.Symbol) || string.IsNullOrWhiteSpace(Currency.Code))
            {
                MessageBox.Show("يرجى ملء الحقول الأساسية (الاسم، الرمز، الكود).", "بيانات ناقصة");
                return;
            }

            try
            {
                await _currencyService.SaveCurrencyAsync(Currency);
                MessageBox.Show("تم حفظ العملة بنجاح.", "نجاح");
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ العملة: {ex.Message}", "خطأ");
            }
        }
    }
}