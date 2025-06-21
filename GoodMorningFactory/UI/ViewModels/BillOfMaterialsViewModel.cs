// GoodMorningFactory/UI/ViewModels/BillOfMaterialsViewModel.cs
// *** الكود الكامل والمعدل - تم تفعيل دالة الطباعة ***
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
    public class BillOfMaterialsViewModel : BaseViewModel
    {
        private readonly IBomService _bomService;
        private readonly IPrintingService _printingService;

        public ObservableCollection<BillOfMaterials> Boms { get; } = new ObservableCollection<BillOfMaterials>();

        public ICommand AddBomCommand { get; }
        public ICommand EditBomCommand { get; }
        public ICommand CopyBomCommand { get; }
        public ICommand PrintBomCommand { get; }
        public ICommand DeleteBomCommand { get; }
        public ICommand RefreshCommand { get; }

        public BillOfMaterialsViewModel()
        {
            _bomService = new BomService();
            _printingService = new PrintingService();

            AddBomCommand = new RelayCommand(AddBom);
            EditBomCommand = new RelayCommand(EditBom, CanExecute);
            CopyBomCommand = new RelayCommand(CopyBom, CanExecute);
            PrintBomCommand = new RelayCommand(async (p) => await PrintBomAsync(p), CanExecute);
            DeleteBomCommand = new RelayCommand(async (p) => await DeleteBomAsync(p), CanExecute);
            RefreshCommand = new RelayCommand(async (p) => await LoadBomsAsync());

            LoadBomsAsync();
        }

        private async Task LoadBomsAsync()
        {
            try
            {
                var bomsList = await _bomService.GetBomsAsync();
                Boms.Clear();
                foreach (var bom in bomsList)
                {
                    Boms.Add(bom);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل قوائم المكونات: {ex.Message}", "خطأ");
            }
        }

        private void AddBom(object parameter)
        {
            var addWindow = new AddEditBillOfMaterialsWindow();
            if (addWindow.ShowDialog() == true) LoadBomsAsync();
        }

        private void EditBom(object parameter)
        {
            if (parameter is BillOfMaterials bom)
            {
                var editWindow = new AddEditBillOfMaterialsWindow(bomId: bom.Id);
                if (editWindow.ShowDialog() == true) LoadBomsAsync();
            }
        }

        private void CopyBom(object parameter)
        {
            if (parameter is BillOfMaterials bomToCopy)
            {
                var copyWindow = new AddEditBillOfMaterialsWindow(sourceBomIdToCopy: bomToCopy.Id);
                if (copyWindow.ShowDialog() == true) LoadBomsAsync();
            }
        }

        // ======================= بداية الإصلاح الرئيسي =======================
        private async Task PrintBomAsync(object parameter)
        {
            if (parameter is BillOfMaterials bom)
            {
                // الآن يتم استدعاء خدمة الطباعة بشكل صحيح
                await _printingService.PrintBomAsync(bom.Id);
            }
        }
        // ======================== نهاية الإصلاح الرئيسي ========================

        private async Task DeleteBomAsync(object parameter)
        {
            if (parameter is BillOfMaterials bom)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف قائمة المكونات للمنتج '{bom.FinishedGood.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _bomService.DeleteBomAsync(bom.Id);
                        Boms.Remove(bom);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}", "خطأ");
                    }
                }
            }
        }

        private bool CanExecute(object parameter) => parameter != null;
    }
}