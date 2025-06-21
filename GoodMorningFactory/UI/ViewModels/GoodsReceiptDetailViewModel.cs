// GoodMorningFactory/UI/ViewModels/GoodsReceiptDetailViewModel.cs
// *** ملف جديد: ViewModel لنافذة تفاصيل سند الاستلام ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class GoodsReceiptDetailViewModel : BaseViewModel
    {
        private readonly IGoodsReceiptService _grnService;
        private readonly int _grnId;

        private GoodsReceiptNote _goodsReceiptNote;
        public GoodsReceiptNote GoodsReceiptNote
        {
            get => _goodsReceiptNote;
            set { _goodsReceiptNote = value; OnPropertyChanged(); }
        }

        public RelayCommand CreateInvoiceCommand { get; }

        public GoodsReceiptDetailViewModel(int grnId)
        {
            _grnId = grnId;
            _grnService = new GoodsReceiptService();
            CreateInvoiceCommand = new RelayCommand(CreateInvoice, CanCreateInvoice);
            LoadDetailsAsync();
        }

        private async void LoadDetailsAsync()
        {
            try
            {
                GoodsReceiptNote = await _grnService.GetGoodsReceiptByIdAsync(_grnId);
                CreateInvoiceCommand.RaiseCanExecuteChanged(); // تحديث حالة الزر بعد تحميل البيانات
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل التفاصيل: {ex.Message}", "خطأ");
            }
        }

        private bool CanCreateInvoice(object parameter)
        {
            // الزر يكون فعالاً فقط إذا لم يتم فوترة سند الاستلام بعد
            return GoodsReceiptNote != null && !GoodsReceiptNote.IsInvoiced;
        }

        private void CreateInvoice(object parameter)
        {
            var invoiceWindow = new AddEditPurchaseInvoiceWindow(grnId: _grnId);
            if (invoiceWindow.ShowDialog() == true)
            {
                // إعادة تحميل البيانات لتحديث حالة الزر
                LoadDetailsAsync();
            }
        }
    }
}