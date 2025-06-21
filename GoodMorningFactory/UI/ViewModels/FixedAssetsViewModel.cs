// GoodMorningFactory/UI/ViewModels/FixedAssetsViewModel.cs
// *** ملف جديد: ViewModel لواجهة عرض الأصول الثابتة ***
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
    public class FixedAssetsViewModel : BaseViewModel
    {
        private readonly IFixedAssetService _assetService;
        public ObservableCollection<FixedAsset> Assets { get; } = new ObservableCollection<FixedAsset>();

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RunDepreciationCommand { get; } // للمرحلة الثانية

        public FixedAssetsViewModel()
        {
            _assetService = new FixedAssetService();
            AddCommand = new RelayCommand(AddAsset);
            EditCommand = new RelayCommand(EditAsset, CanExecute);
            DeleteCommand = new RelayCommand(async (p) => await DeleteAssetAsync(p), CanExecute);
            RunDepreciationCommand = new RelayCommand(RunDepreciation); // سيتم تنفيذها لاحقاً

            LoadDataAsync();
        }

        private bool CanExecute(object parameter) => parameter != null;

        private async void LoadDataAsync()
        {
            try
            {
                var assetsList = await _assetService.GetAssetsAsync();
                Assets.Clear();
                foreach (var asset in assetsList)
                {
                    Assets.Add(asset);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private void AddAsset(object parameter)
        {
            var addWindow = new AddEditFixedAssetWindow();
            if (addWindow.ShowDialog() == true) LoadDataAsync();
        }

        private void EditAsset(object parameter)
        {
            if (parameter is FixedAsset asset)
            {
                var editWindow = new AddEditFixedAssetWindow(asset.Id);
                if (editWindow.ShowDialog() == true) LoadDataAsync();
            }
        }

        private async Task DeleteAssetAsync(object parameter)
        {
            if (parameter is FixedAsset asset)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف الأصل '{asset.AssetName}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _assetService.DeleteAssetAsync(asset.Id);
                        Assets.Remove(asset);
                    }
                    catch (InvalidOperationException ex)
                    {
                        MessageBox.Show(ex.Message, "عملية مرفوضة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}", "خطأ");
                    }
                }
            }
        }

        private void RunDepreciation(object parameter)
        {
            // سيتم تطوير هذا الجزء في المرحلة الثانية
            var depreciationWindow = new RunDepreciationWindow();
            depreciationWindow.ShowDialog();
        }
    }
}