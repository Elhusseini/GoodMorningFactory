// GoodMorningFactory/UI/ViewModels/AddSalesReturnViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddSalesReturnViewModel : BaseViewModel
    {
        private readonly ISalesReturnService _salesReturnService; // <-- استخدام الخدمة الجديدة
        private readonly int _saleId;

        public string InvoiceNumber { get; private set; }
        public ObservableCollection<SalesReturnItemViewModel> ItemsToReturn { get; set; }
        public RelayCommand ConfirmReturnCommand { get; }

        public AddSalesReturnViewModel(int saleId)
        {
            _salesReturnService = new SalesReturnService(); // <-- تهيئة الخدمة
            _saleId = saleId;
            ItemsToReturn = new ObservableCollection<SalesReturnItemViewModel>();
            ConfirmReturnCommand = new RelayCommand(async (param) => await ConfirmReturn(param), CanConfirmReturn);
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                var sale = await _salesReturnService.GetDataForReturnCreationAsync(_saleId);
                if (sale == null) return;

                InvoiceNumber = $"مرتجع من فاتورة رقم: {sale.InvoiceNumber}";
                OnPropertyChanged(nameof(InvoiceNumber));

                var previouslyReturned = await _salesReturnService.GetPreviouslyReturnedQuantitiesAsync(_saleId);

                foreach (var item in sale.SaleItems)
                {
                    int returnedQty = previouslyReturned.ContainsKey(item.ProductId) ? previouslyReturned[item.ProductId] : 0;
                    if (item.Quantity > returnedQty) // نعرض فقط البنود التي يمكن إرجاعها
                    {
                        ItemsToReturn.Add(new SalesReturnItemViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Product.Name,
                            OriginalQuantity = item.Quantity,
                            PreviouslyReturnedQuantity = returnedQty,
                            QuantityToReturn = 0,
                            UnitPrice = item.UnitPrice
                        });
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private bool CanConfirmReturn(object obj)
        {
            return ItemsToReturn != null && ItemsToReturn.Any(i => i.QuantityToReturn > 0);
        }

        private async Task ConfirmReturn(object parameter)
        {
            try
            {
                var validItems = ItemsToReturn.Where(i => i.QuantityToReturn > 0).ToList();
                await _salesReturnService.CreateSalesReturnAsync(_saleId, validItems);
                MessageBox.Show("تم تسجيل المرتجع بنجاح.", "نجاح");

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
            }
        }
    }
}