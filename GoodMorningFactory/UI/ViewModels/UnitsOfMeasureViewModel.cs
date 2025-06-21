// GoodMorningFactory/UI/ViewModels/UnitsOfMeasureViewModel.cs
// *** ملف جديد: ViewModel لواجهة عرض وحدات القياس ***
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
    public class UnitsOfMeasureViewModel : BaseViewModel
    {
        private readonly IUnitOfMeasureService _uomService;

        public ObservableCollection<UnitOfMeasure> Uoms { get; set; } = new ObservableCollection<UnitOfMeasure>();

        public ICommand AddUomCommand { get; }
        public ICommand EditUomCommand { get; }
        public ICommand DeleteUomCommand { get; }

        public UnitsOfMeasureViewModel()
        {
            _uomService = new UnitOfMeasureService();

            AddUomCommand = new RelayCommand(AddUom);
            EditUomCommand = new RelayCommand(EditUom);
            DeleteUomCommand = new RelayCommand(DeleteUom);

            LoadUomsAsync();
        }

        private async void LoadUomsAsync()
        {
            try
            {
                var uomsList = await _uomService.GetUomsAsync();
                Uoms.Clear();
                foreach (var uom in uomsList)
                {
                    Uoms.Add(uom);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل وحدات القياس: {ex.Message}");
            }
        }

        private void AddUom(object parameter)
        {
            var addWindow = new AddEditUomWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadUomsAsync();
            }
        }

        private void EditUom(object parameter)
        {
            if (parameter is UnitOfMeasure uom)
            {
                var editWindow = new AddEditUomWindow(uom.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadUomsAsync();
                }
            }
        }

        private async void DeleteUom(object parameter)
        {
            if (parameter is UnitOfMeasure uom)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف وحدة القياس '{uom.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    await _uomService.DeleteUomAsync(uom.Id);
                    LoadUomsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}