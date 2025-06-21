// GoodMorningFactory/UI/ViewModels/AddEditUomViewModel.cs
// *** الكود الكامل والمراجع ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditUomViewModel : BaseViewModel
    {
        private readonly IUnitOfMeasureService _uomService;
        private UnitOfMeasure _uom;

        public UnitOfMeasure Uom
        {
            get => _uom;
            set { _uom = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; }
        public RelayCommand SaveCommand { get; }

        public AddEditUomViewModel(IUnitOfMeasureService uomService, int? uomId = null)
        {
            _uomService = uomService;
            SaveCommand = new RelayCommand(async (param) => await SaveAsync(param as Window));
            LoadData(uomId);
        }

        private async void LoadData(int? uomId)
        {
            if (uomId.HasValue && uomId != 0)
            {
                Uom = await _uomService.GetUomByIdAsync(uomId.Value);
                WindowTitle = "تعديل وحدة قياس";
            }
            else
            {
                Uom = new UnitOfMeasure();
                WindowTitle = "إضافة وحدة قياس جديدة";
            }
            OnPropertyChanged(nameof(WindowTitle));
        }

        private async Task SaveAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(Uom.Name) || string.IsNullOrWhiteSpace(Uom.Abbreviation))
            {
                MessageBox.Show("اسم الوحدة والاختصار حقول مطلوبة.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _uomService.SaveUomAsync(Uom);
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الحفظ: {ex.Message}", "خطأ");
            }
        }
    }
}