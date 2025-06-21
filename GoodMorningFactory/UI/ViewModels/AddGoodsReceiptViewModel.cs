// GoodMorningFactory/UI/ViewModels/AddGoodsReceiptViewModel.cs
// *** الكود الكامل والنهائي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddGoodsReceiptViewModel : BaseViewModel
    {
        private readonly int _purchaseOrderId;
        private readonly IGoodsReceiptService _goodsReceiptService;

        private string _purchaseOrderNumberText;
        public string PurchaseOrderNumberText { get => _purchaseOrderNumberText; set { _purchaseOrderNumberText = value; OnPropertyChanged(); } }

        public ObservableCollection<GoodsReceiptItemViewModel> ItemsToReceive { get; set; }

        public RelayCommand ConfirmReceiptCommand { get; }
        public RelayCommand EnterTrackingDataCommand { get; }

        public AddGoodsReceiptViewModel(int purchaseOrderId)
        {
            _purchaseOrderId = purchaseOrderId;
            _goodsReceiptService = new GoodsReceiptService();
            ItemsToReceive = new ObservableCollection<GoodsReceiptItemViewModel>();

            ConfirmReceiptCommand = new RelayCommand(ConfirmReceipt, CanConfirmReceipt);
            EnterTrackingDataCommand = new RelayCommand(EnterTrackingData);

            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var po = await db.PurchaseOrders.FindAsync(_purchaseOrderId);
                    if (po == null) return;
                    PurchaseOrderNumberText = $"خاص بأمر الشراء رقم: {po.PurchaseOrderNumber}";
                }

                var items = await _goodsReceiptService.GetDataForReceiptCreationAsync(_purchaseOrderId);
                ItemsToReceive = new ObservableCollection<GoodsReceiptItemViewModel>(items);
                OnPropertyChanged(nameof(ItemsToReceive));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private void EnterTrackingData(object parameter)
        {
            if (parameter is GoodsReceiptItemViewModel item)
            {
                if (item.QuantityReceived <= 0)
                {
                    MessageBox.Show("يرجى إدخال الكمية المستلمة أولاً.", "تنبيه");
                    return;
                }
                var trackingWindow = new EnterTrackingDataWindow(item.TrackingMethod, item.QuantityReceived);
                if (trackingWindow.ShowDialog() == true)
                {
                    if (item.TrackingMethod == ProductTrackingMethod.BySerialNumber)
                        item.SerialNumbers = trackingWindow.SerialNumbers.ToList();
                    else
                        item.LotInfo = new LotNumberInfo { Value = trackingWindow.LotNumber, ExpiryDate = trackingWindow.ExpiryDate };
                }
            }
        }

        private bool CanConfirmReceipt(object arg) => ItemsToReceive.Any(i => i.QuantityReceived > 0);

        private async void ConfirmReceipt(object parameter)
        {
            try
            {
                var itemsToProcess = ItemsToReceive.Where(i => i.QuantityReceived > 0).ToList();
                await _goodsReceiptService.SaveGoodsReceiptAsync(_purchaseOrderId, itemsToProcess);

                MessageBox.Show("تم تسجيل استلام البضاعة بنجاح.", "نجاح");
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت العملية: {ex.Message}\n\nالتفاصيل: {ex.InnerException?.Message}", "خطأ");
            }
        }
    }
}