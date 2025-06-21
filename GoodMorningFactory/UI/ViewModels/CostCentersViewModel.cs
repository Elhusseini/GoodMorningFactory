// GoodMorningFactory/UI/ViewModels/CostCentersViewModel.cs
// *** ملف جديد: ViewModel لواجهة عرض مراكز التكلفة ***
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
    public class CostCentersViewModel : BaseViewModel
    {
        private readonly ICostCenterService _costCenterService;
        public ObservableCollection<CostCenter> CostCenters { get; } = new ObservableCollection<CostCenter>();

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public CostCentersViewModel()
        {
            _costCenterService = new CostCenterService();
            AddCommand = new RelayCommand(AddCostCenter);
            EditCommand = new RelayCommand(EditCostCenter, CanExecute);
            DeleteCommand = new RelayCommand(async (p) => await DeleteCostCenterAsync(p), CanExecute);

            LoadDataAsync();
        }

        private bool CanExecute(object parameter) => parameter != null;

        private async void LoadDataAsync()
        {
            try
            {
                var centers = await _costCenterService.GetCostCentersAsync();
                CostCenters.Clear();
                foreach (var center in centers)
                {
                    CostCenters.Add(center);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private void AddCostCenter(object parameter)
        {
            var addWindow = new AddEditCostCenterWindow();
            if (addWindow.ShowDialog() == true) LoadDataAsync();
        }

        private void EditCostCenter(object parameter)
        {
            if (parameter is CostCenter center)
            {
                var editWindow = new AddEditCostCenterWindow(center.Id);
                if (editWindow.ShowDialog() == true) LoadDataAsync();
            }
        }

        private async Task DeleteCostCenterAsync(object parameter)
        {
            if (parameter is CostCenter center)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف مركز التكلفة '{center.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _costCenterService.DeleteCostCenterAsync(center.Id);
                        CostCenters.Remove(center);
                    }
                    catch (InvalidOperationException ex) // التقاط الخطأ المخصص من الخدمة
                    {
                        MessageBox.Show(ex.Message, "عملية مرفوضة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (Exception ex) // التقاط أي أخطاء أخرى
                    {
                        MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}", "خطأ");
                    }
                }
            }
        }
    }
}