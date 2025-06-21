// GoodMorningFactory/UI/ViewModels/PurchaseReturnsViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PurchaseReturnDisplayViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; }
        public DateTime ReturnDate { get; set; }
        public string SupplierName { get; set; }
        public decimal TotalAmount { get; set; }
        public PurchaseReturnStatus Status { get; set; }
        public string TotalAmountFormatted => $"{TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
    }

    public class PurchaseReturnsViewModel : BaseViewModel
    {
        private readonly IPurchaseReturnService _returnService;

        private ObservableCollection<PurchaseReturnDisplayViewModel> _purchaseReturns;
        public ObservableCollection<PurchaseReturnDisplayViewModel> PurchaseReturns
        {
            get => _purchaseReturns;
            set { _purchaseReturns = value; OnPropertyChanged(); }
        }

        public RelayCommand AddPurchaseReturnCommand { get; }

        public PurchaseReturnsViewModel()
        {
            _returnService = new PurchaseReturnService();
            AddPurchaseReturnCommand = new RelayCommand(AddPurchaseReturn);
            LoadPurchaseReturns();
        }

        private async void LoadPurchaseReturns()
        {
            try
            {
                var returnsFromDb = await _returnService.GetPurchaseReturnsAsync();
                var displayList = returnsFromDb.Select(pr => new PurchaseReturnDisplayViewModel
                {
                    Id = pr.Id,
                    ReturnNumber = pr.ReturnNumber,
                    ReturnDate = pr.ReturnDate,
                    SupplierName = pr.Purchase?.Supplier?.Name ?? "غير معروف",
                    TotalAmount = pr.TotalReturnValue,
                    Status = pr.Status
                });
                PurchaseReturns = new ObservableCollection<PurchaseReturnDisplayViewModel>(displayList);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"فشل تحميل مرتجعات المشتريات: {ex.Message}", "خطأ");
            }
        }

        private void AddPurchaseReturn(object obj)
        {
            var addReturnWindow = new AddPurchaseReturnWindow();
            if (addReturnWindow.ShowDialog() == true)
            {
                LoadPurchaseReturns();
            }
        }
    }
}