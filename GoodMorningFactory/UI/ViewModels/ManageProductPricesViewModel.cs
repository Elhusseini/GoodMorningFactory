// GoodMorningFactory/UI/ViewModels/ManageProductPricesViewModel.cs
// *** الكود الكامل والصحيح لـ ViewModel نافذة إدارة الأسعار ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ManageProductPricesViewModel : BaseViewModel
    {
        private readonly IPriceListService _priceListService;
        private readonly int _priceListId;
        private PriceList _priceList;

        public string WindowTitle { get; private set; }

        public ObservableCollection<ProductPrice> ProductPrices { get; set; }

        public ICommand SaveCommand { get; }

        public ManageProductPricesViewModel(IPriceListService service, int priceListId)
        {
            _priceListService = service;
            _priceListId = priceListId;
            ProductPrices = new ObservableCollection<ProductPrice>();
            SaveCommand = new RelayCommand(async (p) => await SaveAsync(p as Window));
            LoadDataAsync();
        }

        public ManageProductPricesViewModel()
        {
            WindowTitle = "إدارة أسعار المنتجات (وضع التصميم)";
            ProductPrices = new ObservableCollection<ProductPrice>
            {
                 new ProductPrice { Product = new Product { ProductCode = "P001", Name = "منتج تصميم 1", SalePrice = 100 }, Price = 95 },
                 new ProductPrice { Product = new Product { ProductCode = "P002", Name = "منتج تصميم 2", SalePrice = 250 }, Price = 250 }
            };
        }

        private async void LoadDataAsync()
        {
            try
            {
                _priceList = await _priceListService.GetPriceListByIdAsync(_priceListId);
                if (_priceList == null)
                {
                    MessageBox.Show("لم يتم العثور على قائمة الأسعار.", "خطأ");
                    return;
                }

                WindowTitle = $"أسعار المنتجات في قائمة: {_priceList.Name}";
                OnPropertyChanged(nameof(WindowTitle));

                ProductPrices.Clear();
                // إضافة أسعار المنتجات الموجودة مسبقًا
                foreach (var price in _priceList.ProductPrices)
                {
                    ProductPrices.Add(price);
                }

                // جلب كل المنتجات المتاحة وإضافة التي ليست في القائمة بسعر 0
                var allProducts = await _priceListService.GetAvailableProductsAsync();
                foreach (var product in allProducts)
                {
                    if (!ProductPrices.Any(p => p.ProductId == product.Id))
                    {
                        ProductPrices.Add(new ProductPrice
                        {
                            PriceListId = _priceListId,
                            ProductId = product.Id,
                            Product = product,
                            Price = 0 // سعر صفر يعني أنه غير محدد في هذه القائمة
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private async Task SaveAsync(Window window)
        {
            try
            {
                // نرسل فقط الأسعار التي لها قيمة (أكبر من صفر)
                var pricesToSave = ProductPrices.Where(p => p.Price > 0).ToList();
                await _priceListService.SaveProductPricesAsync(_priceListId, pricesToSave);
                MessageBox.Show("تم حفظ التغييرات بنجاح.", "نجاح");
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الأسعار: {ex.Message}", "خطأ");
            }
        }
    }
}