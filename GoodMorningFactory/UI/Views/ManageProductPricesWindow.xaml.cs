// UI/Views/ManageProductPricesWindow.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    // ViewModel خاص لعرض سعر المنتج في قائمة معينة
    public class ProductPriceViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal DefaultSalePrice { get; set; }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged(nameof(Price));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class ManageProductPricesWindow : Window
    {
        private readonly int _priceListId;
        private List<ProductPriceViewModel> _productPrices;

        public ManageProductPricesWindow(int priceListId)
        {
            InitializeComponent();
            _priceListId = priceListId;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var priceList = db.PriceLists.Find(_priceListId);
                    if (priceList == null)
                    {
                        MessageBox.Show("لم يتم العثور على قائمة الأسعار.");
                        this.Close();
                        return;
                    }
                    PriceListNameTextBlock.Text = $"أسعار المنتجات في قائمة: {priceList.Name}";

                    // جلب جميع المنتجات التي يمكن بيعها
                    var allProducts = db.Products.Where(p => p.ProductType == ProductType.FinishedGood || p.ProductType == ProductType.WorkInProgress).ToList();

                    // جلب الأسعار المحددة مسبقاً في هذه القائمة
                    var existingPrices = db.ProductPrices
                                           .Where(pp => pp.PriceListId == _priceListId)
                                           .ToDictionary(pp => pp.ProductId, pp => pp.Price);

                    _productPrices = new List<ProductPriceViewModel>();
                    foreach (var product in allProducts)
                    {
                        _productPrices.Add(new ProductPriceViewModel
                        {
                            ProductId = product.Id,
                            ProductCode = product.ProductCode,
                            ProductName = product.Name,
                            DefaultSalePrice = product.SalePrice,
                            Price = existingPrices.ContainsKey(product.Id) ? existingPrices[product.Id] : 0 // السعر صفر إذا لم يحدد
                        });
                    }
                    ProductPricesDataGrid.ItemsSource = _productPrices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    foreach (var item in _productPrices)
                    {
                        var existingPrice = db.ProductPrices.FirstOrDefault(pp => pp.PriceListId == _priceListId && pp.ProductId == item.ProductId);

                        if (item.Price > 0) // إذا تم تحديد سعر
                        {
                            if (existingPrice != null)
                            {
                                // تحديث السعر الموجود
                                existingPrice.Price = item.Price;
                            }
                            else
                            {
                                // إضافة سعر جديد
                                db.ProductPrices.Add(new ProductPrice
                                {
                                    PriceListId = _priceListId,
                                    ProductId = item.ProductId,
                                    Price = item.Price
                                });
                            }
                        }
                        else // إذا كان السعر صفراً أو فارغاً
                        {
                            if (existingPrice != null)
                            {
                                // حذف السعر الموجود
                                db.ProductPrices.Remove(existingPrice);
                            }
                        }
                    }
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ أسعار المنتجات بنجاح.", "نجاح");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الأسعار: {ex.Message}", "خطأ");
            }
        }
    }
}
