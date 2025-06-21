// GoodMorningFactory/UI/ViewModels/AddEditSalesQuotationViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditSalesQuotationViewModel : INotifyPropertyChanged
    {
        #region الخصائص والخدمات
        private readonly INumberingService _numberingService; // <-- تم التصحيح لاستخدام الواجهة
        private SalesQuotation _quotation;
        private string _windowTitle;
        private string _searchProductText;
        private string _totalAmountText;

        public SalesQuotation Quotation { get => _quotation; set { _quotation = value; OnPropertyChanged(nameof(Quotation)); } }
        public string WindowTitle { get => _windowTitle; set { _windowTitle = value; OnPropertyChanged(nameof(WindowTitle)); } }
        public ObservableCollection<SalesQuotationItemViewModel> Items { get; set; }
        public ObservableCollection<Customer> Customers { get; set; }
        public ObservableCollection<PriceList> PriceLists { get; set; }

        public string SearchProductText { get => _searchProductText; set { _searchProductText = value; OnPropertyChanged(nameof(SearchProductText)); } }
        public string TotalAmountText { get => _totalAmountText; set { _totalAmountText = value; OnPropertyChanged(nameof(TotalAmountText)); } }
        #endregion

        #region الأوامر (Commands)
        public ICommand AddProductCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SaveCommand { get; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public AddEditSalesQuotationViewModel(int? quotationId = null, Opportunity sourceOpportunity = null)
        {
            _numberingService = new NumberingService(); // <-- تم التصحيح
            Items = new ObservableCollection<SalesQuotationItemViewModel>();
            Customers = new ObservableCollection<Customer>();
            PriceLists = new ObservableCollection<PriceList>();

            AddProductCommand = new RelayCommand(async _ => await AddProductAsync(), _ => !string.IsNullOrWhiteSpace(SearchProductText));
            RemoveItemCommand = new RelayCommand(RemoveItem);
            SaveCommand = new RelayCommand(async param => await SaveAsync(param as Window, sourceOpportunity), _ => Quotation?.Customer != null && Items.Any());

            Items.CollectionChanged += (s, e) => UpdateTotal();

            Task.Run(() => LoadInitialData(quotationId, sourceOpportunity));
        }

        private async Task LoadInitialData(int? quotationId, Opportunity sourceOpportunity)
        {
            using (var db = new DatabaseContext())
            {
                var customers = await db.Customers.Where(c => c.IsActive).ToListAsync();
                var priceLists = await db.PriceLists.ToListAsync();
                // استخدام الـ Dispatcher ضروري لتحديث الواجهة من Thread مختلف
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var c in customers) Customers.Add(c);
                    foreach (var pl in priceLists) PriceLists.Add(pl);
                });
            }

            if (quotationId.HasValue) // وضع التعديل
            {
                WindowTitle = "تعديل عرض سعر";
                using (var db = new DatabaseContext())
                {
                    Quotation = await db.SalesQuotations
                                          .Include(q => q.Customer)
                                          .Include(q => q.SalesQuotationItems).ThenInclude(i => i.Product)
                                          .FirstOrDefaultAsync(q => q.Id == quotationId.Value);
                    if (Quotation != null)
                    {
                        foreach (var item in Quotation.SalesQuotationItems)
                        {
                            var itemVm = new SalesQuotationItemViewModel
                            {
                                ProductId = item.ProductId,
                                ProductName = item.Product.Name,
                                Description = item.Description,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                Discount = item.Discount
                            };
                            itemVm.PropertyChanged += (s, e) => UpdateTotal();
                            Items.Add(itemVm);
                        }
                    }
                }
            }
            else // وضع الإضافة
            {
                WindowTitle = "إنشاء عرض سعر جديد";
                Quotation = new SalesQuotation
                {
                    QuotationNumber = await _numberingService.GetNextNumberAsync(DocumentType.SalesQuotation),
                    QuotationDate = DateTime.Today,
                    ValidUntilDate = DateTime.Today.AddDays(30),
                    Status = QuotationStatus.Draft
                };

                // إذا كان المصدر هو فرصة بيعية، قم بتعبئة البيانات منها
                if (sourceOpportunity != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Quotation.Customer = Customers.FirstOrDefault(c => c.Id == sourceOpportunity.CustomerId);
                    });
                    Quotation.CustomerId = sourceOpportunity.CustomerId;
                    Quotation.Notes = $"تم إنشاء هذا العرض بناءً على الفرصة البيعية: '{sourceOpportunity.Name}'";
                }
            }
            UpdateTotal();
        }

        private async Task AddProductAsync()
        {
            using (var db = new DatabaseContext())
            {
                var searchTextLower = SearchProductText.ToLower();
                var product = await db.Products.FirstOrDefaultAsync(p => p.ProductCode.ToLower() == searchTextLower || p.Name.ToLower().Contains(searchTextLower));
                if (product != null)
                {
                    var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
                    if (existingItem != null) { existingItem.Quantity++; }
                    else
                    {
                        var newItem = new SalesQuotationItemViewModel
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Description = product.Description ?? product.Name,
                            Quantity = 1,
                            UnitPrice = product.SalePrice,
                            Discount = 0
                        };
                        newItem.PropertyChanged += (s, e) => UpdateTotal();
                        Items.Add(newItem);
                    }
                    SearchProductText = string.Empty;
                }
                else { MessageBox.Show("لم يتم العثور على المنتج.", "بحث", MessageBoxButton.OK, MessageBoxImage.Information); }
            }
        }

        private void RemoveItem(object parameter) { if (parameter is SalesQuotationItemViewModel item) { Items.Remove(item); } }

        private void UpdateTotal()
        {
            if (Quotation == null) return;
            Quotation.Subtotal = Items.Sum(i => i.Subtotal);
            Quotation.TaxAmount = 0; // يمكنك إضافة منطق حساب الضريبة هنا
            Quotation.TotalAmount = Quotation.Subtotal + Quotation.TaxAmount;
            // استخدام كلاس الإعدادات الصحيح الذي زودتني به
            TotalAmountText = $"{Quotation.TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
        }

        private async Task SaveAsync(Window window, Opportunity sourceOpportunity)
        {
            using (var db = new DatabaseContext())
            {
                // ربط العميل بشكل صحيح قبل الحفظ
                Quotation.Customer = null;

                // تحويل بنود الـ ViewModel إلى بنود Model
                Quotation.SalesQuotationItems = new Collection<SalesQuotationItem>(Items.Select(vm => new SalesQuotationItem
                {
                    Id = 0, // دائماً Id جديد للبنود
                    SalesQuotationId = Quotation.Id,
                    ProductId = vm.ProductId,
                    Description = vm.Description,
                    Quantity = vm.Quantity,
                    UnitPrice = vm.UnitPrice,
                    Discount = vm.Discount
                }).ToList());

                if (Quotation.Id > 0) // وضع التعديل
                {
                    var existingItems = db.SalesQuotationItems.Where(i => i.SalesQuotationId == Quotation.Id);
                    db.SalesQuotationItems.RemoveRange(existingItems);
                    db.Entry(Quotation).State = EntityState.Modified;
                }
                else // وضع الإضافة
                {
                    db.SalesQuotations.Add(Quotation);
                }

                // حفظ التغييرات الأولية للحصول على ID لعرض السعر
                await db.SaveChangesAsync();

                // تحديث الفرصة بعد الحصول على ID لعرض السعر
                if (sourceOpportunity != null && Quotation.Id > 0 && sourceOpportunity.GeneratedQuotationId == null)
                {
                    var opportunityInDb = await db.Opportunities.FindAsync(sourceOpportunity.Id);
                    if (opportunityInDb != null)
                    {
                        opportunityInDb.GeneratedQuotationId = Quotation.Id;
                        await db.SaveChangesAsync();
                    }
                }
            }
            window.DialogResult = true;
            window.Close();
        }

        protected void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}