// UI/Views/GoodsReceiptDetailWindow.xaml.cs
// *** الكود الكامل للكود الخلفي لنافذة تفاصيل مذكرة الاستلام مع الإصلاح ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class GoodsReceiptDetailWindow : Window
    {
        private readonly int _grnId;

        public GoodsReceiptDetailWindow(int grnId)
        {
            InitializeComponent();
            _grnId = grnId;
            LoadDetails();
        }

        private void LoadDetails()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var grn = db.GoodsReceiptNotes
                                .Include(g => g.PurchaseOrder)
                                .Include(g => g.GoodsReceiptNoteItems)
                                .ThenInclude(gi => gi.Product)
                                .FirstOrDefault(g => g.Id == _grnId);

                    if (grn == null) { this.Close(); return; }

                    GrnNumberTextBlock.Text = grn.GRNNumber;
                    PoNumberTextBlock.Text = grn.PurchaseOrder.PurchaseOrderNumber;
                    ItemsDataGrid.ItemsSource = grn.GoodsReceiptNoteItems;
                    CreateInvoiceButton.IsEnabled = !grn.IsInvoiced;
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ: {ex.Message}"); }
        }

        // --- بداية الإصلاح: استدعاء المُنشئ الصحيح مع تمرير grnId ---
        private void CreateInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            var invoiceWindow = new AddEditPurchaseInvoiceWindow(grnId: _grnId);
            if (invoiceWindow.ShowDialog() == true)
            {
                CreateInvoiceButton.IsEnabled = false; // تعطيل الزر بعد إنشاء الفاتورة
            }
        }
        // --- نهاية الإصلاح ---
    }
}