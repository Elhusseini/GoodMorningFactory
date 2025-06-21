// GoodMorningFactory/UI/ViewModels/AddEditPriceListViewModel.cs
// *** الكود الكامل لـ ViewModel نافذة الإضافة والتعديل ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditPriceListViewModel : BaseViewModel
    {
        private readonly IPriceListService _priceListService;
        private PriceList _priceList;

        public PriceList PriceList
        {
            get => _priceList;
            set { _priceList = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; }
        public RelayCommand SaveCommand { get; }

        public AddEditPriceListViewModel(IPriceListService service, int? priceListId)
        {
            _priceListService = service;
            SaveCommand = new RelayCommand(async (p) => await SaveAsync(p as Window));
            LoadData(priceListId);
        }

        private async void LoadData(int? priceListId)
        {
            if (priceListId.HasValue && priceListId != 0)
            {
                WindowTitle = "تعديل قائمة أسعار";
                PriceList = await _priceListService.GetPriceListByIdAsync(priceListId.Value);
            }
            else
            {
                WindowTitle = "إضافة قائمة أسعار جديدة";
                PriceList = new PriceList();
            }
            OnPropertyChanged(nameof(WindowTitle));
        }

        private async Task SaveAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(PriceList.Name))
            {
                MessageBox.Show("اسم القائمة حقل مطلوب.", "بيانات ناقصة");
                return;
            }
            await _priceListService.SavePriceListAsync(PriceList);
            window.DialogResult = true;
        }
    }
}