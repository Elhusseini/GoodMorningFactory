// GoodMorningFactory/UI/ViewModels/LowStockNotificationsViewModel.cs
// *** ملف جديد: ViewModel لواجهة تنبيهات انخفاض المخزون ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class LowStockNotificationsViewModel : BaseViewModel
    {
        private readonly IInventoryService _inventoryService;

        public ObservableCollection<LowStockNotificationViewModel> LowStockItems { get; } = new ObservableCollection<LowStockNotificationViewModel>();

        public bool HasLowStockItems => LowStockItems.Count > 0;

        public ICommand RefreshCommand { get; }

        public LowStockNotificationsViewModel()
        {
            _inventoryService = new InventoryService();
            RefreshCommand = new RelayCommand(async (p) => await LoadLowStockItemsAsync());
            LoadLowStockItemsAsync();
        }

        private async Task LoadLowStockItemsAsync()
        {
            try
            {
                var items = await _inventoryService.GetLowStockItemsAsync();
                LowStockItems.Clear();
                foreach (var item in items)
                {
                    LowStockItems.Add(item);
                }
                OnPropertyChanged(nameof(HasLowStockItems)); // إشعار الواجهة بأن الخاصية قد تغيرت
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل التنبيهات: {ex.Message}", "خطأ");
            }
        }
    }
}