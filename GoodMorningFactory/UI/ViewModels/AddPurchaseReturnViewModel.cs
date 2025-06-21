// GoodMorningFactory/UI/ViewModels/AddPurchaseReturnViewModel.cs
// *** الكود الكامل والنهائي - تم إصلاح منطق حساب الكميات بالكامل ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddPurchaseReturnViewModel : BaseViewModel
    {
        private readonly IPurchaseReturnService _returnService;
        private readonly int? _preselectedPurchaseId;

        #region Properties
        private bool _isPurchaseSelectionVisible = true;
        public bool IsPurchaseSelectionVisible { get => _isPurchaseSelectionVisible; set { _isPurchaseSelectionVisible = value; OnPropertyChanged(); } }

        private ObservableCollection<Purchase> _availablePurchases;
        public ObservableCollection<Purchase> AvailablePurchases { get => _availablePurchases; set { _availablePurchases = value; OnPropertyChanged(); } }

        private Purchase _selectedPurchase;
        public Purchase SelectedPurchase { get => _selectedPurchase; set { _selectedPurchase = value; OnPropertyChanged(); } }

        public ObservableCollection<PurchaseReturnItemViewModel> ItemsToReturn { get; set; }

        private string _invoiceNumberText;
        public string InvoiceNumberText { get => _invoiceNumberText; set { _invoiceNumberText = value; OnPropertyChanged(); } }

        private string _supplierNameText;
        public string SupplierNameText { get => _supplierNameText; set { _supplierNameText = value; OnPropertyChanged(); } }

        private string _invoiceDateText;
        public string InvoiceDateText { get => _invoiceDateText; set { _invoiceDateText = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public RelayCommand SelectPurchaseCommand { get; }
        public RelayCommand ConfirmReturnCommand { get; }
        #endregion

        public AddPurchaseReturnViewModel(int? purchaseId)
        {
            _preselectedPurchaseId = purchaseId;
            _returnService = new PurchaseReturnService();
            ItemsToReturn = new ObservableCollection<PurchaseReturnItemViewModel>();

            SelectPurchaseCommand = new RelayCommand(async _ => await SelectPurchase());
            ConfirmReturnCommand = new RelayCommand(ConfirmReturn, CanConfirmReturn);

            Initialize();
        }

        private async void Initialize()
        {
            if (_preselectedPurchaseId.HasValue)
            {
                await LoadInvoiceData(_preselectedPurchaseId.Value);
            }
            else
            {
                await LoadAvailablePurchases();
            }
        }

        private async Task LoadAvailablePurchases()
        {
            var purchases = await _returnService.GetReturnablePurchasesAsync();
            AvailablePurchases = new ObservableCollection<Purchase>(purchases);
            IsPurchaseSelectionVisible = true; // التأكد من إظهار لوحة الاختيار
        }

        private async Task LoadInvoiceData(int purchaseId)
        {
            try
            {
                var purchase = await _returnService.GetPurchaseDetailsForReturnAsync(purchaseId);
                if (purchase == null) throw new Exception("لم يتم العثور على الفاتورة.");

                SelectedPurchase = purchase;
                InvoiceNumberText = purchase.InvoiceNumber;
                SupplierNameText = purchase.Supplier?.Name;
                InvoiceDateText = purchase.PurchaseDate.ToString("dd/MM/yyyy");

                // --- بداية الإصلاح: التأكد من مسح القائمة دائماً قبل تعبئتها ---
                ItemsToReturn.Clear();

                // تجميع بنود الفاتورة الأصلية
                var groupedPurchasedItems = purchase.PurchaseItems
                    .GroupBy(item => item.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(item => item.Quantity));

                // جلب الكميات المرتجعة سابقاً لهذه الفاتورة
                var previouslyReturnedItems = await _returnService.GetReturnedItemsForPurchaseAsync(purchaseId);

                foreach (var purchasedItem in groupedPurchasedItems)
                {
                    int previouslyReturnedQty = previouslyReturnedItems.ContainsKey(purchasedItem.Key) ? previouslyReturnedItems[purchasedItem.Key] : 0;
                    int returnableQty = purchasedItem.Value - previouslyReturnedQty;

                    if (returnableQty > 0)
                    {
                        var productInfo = purchase.PurchaseItems.First(pi => pi.ProductId == purchasedItem.Key);
                        ItemsToReturn.Add(new PurchaseReturnItemViewModel
                        {
                            ProductId = purchasedItem.Key,
                            ProductName = productInfo.Product?.Name,
                            OriginalQuantity = returnableQty, // الكمية القابلة للإرجاع
                            QuantityToReturn = 0,
                            UnitPrice = productInfo.UnitPrice
                        });
                    }
                }

                IsPurchaseSelectionVisible = false;
            }
            catch (Exception ex) { MessageBox.Show($"خطأ في تحميل بيانات الفاتورة: {ex.Message}"); }
        }

        private async Task SelectPurchase()
        {
            if (SelectedPurchase == null)
            {
                MessageBox.Show("الرجاء اختيار فاتورة أولاً.");
                return;
            }
            await LoadInvoiceData(SelectedPurchase.Id);
        }

        private bool CanConfirmReturn(object obj)
        {
            return SelectedPurchase != null && ItemsToReturn.Any(item => item.QuantityToReturn > 0 && item.QuantityToReturn <= item.OriginalQuantity);
        }

        private async void ConfirmReturn(object parameter)
        {
            try
            {
                var items = ItemsToReturn.Where(i => i.QuantityToReturn > 0).ToList();
                if (items.Any(i => i.QuantityToReturn > i.OriginalQuantity))
                {
                    MessageBox.Show("لا يمكن إرجاع كمية أكبر من الكمية المتاحة للإرجاع.", "خطأ في الكمية");
                    return;
                }

                await _returnService.CreatePurchaseReturnAsync(SelectedPurchase.Id, items);

                MessageBox.Show("تم تسجيل المرتجع بنجاح.", "نجاح");
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
            }
        }
    }
}