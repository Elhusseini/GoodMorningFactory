// GoodMorningFactory/UI/ViewModels/ProductPurchaseHistoryViewModel.cs
using GoodMorningFactory.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ProductPurchaseHistoryViewModel : BaseViewModel
    {
        private readonly IProductHistoryService _historyService;

        private string _windowTitle;
        public string WindowTitle { get => _windowTitle; set { _windowTitle = value; OnPropertyChanged(); } }

        private string _productName;
        public string ProductName { get => _productName; set { _productName = value; OnPropertyChanged(); } }

        public ObservableCollection<PurchaseHistoryViewModel> PurchaseHistory { get; set; }

        public ProductPurchaseHistoryViewModel()
        {
            ProductName = "منتج تصميم";
            WindowTitle = "سجل أسعار الشراء - منتج تصميم";
        }

        public ProductPurchaseHistoryViewModel(int productId)
        {
            _historyService = new ProductHistoryService();
            PurchaseHistory = new ObservableCollection<PurchaseHistoryViewModel>();
            LoadData(productId);
        }

        private async void LoadData(int productId)
        {
            try
            {
                var product = await _historyService.GetProductByIdAsync(productId);
                if (product != null)
                {
                    ProductName = product.Name;
                    WindowTitle = $"سجل أسعار الشراء - {product.Name}";
                }

                // --- بداية التعديل: الكود أصبح أبسط بكثير ---
                // نستقبل قائمة الـ ViewModel جاهزة من الخدمة
                var history = await _historyService.GetPurchaseHistoryForProductAsync(productId);

                PurchaseHistory.Clear();
                foreach (var vm in history)
                {
                    PurchaseHistory.Add(vm);
                }
                // --- نهاية التعديل ---
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"فشل تحميل سجل الأسعار: {ex.Message}", "خطأ");
            }
        }
    }
}