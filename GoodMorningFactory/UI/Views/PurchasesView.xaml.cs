// UI/Views/PurchasesView.xaml.cs
// *** الكود الكامل للكود الخلفي لواجهة فواتير الموردين مع جميع الوظائف ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class PurchasesView : UserControl
    {
        public PurchasesView()
        {
            InitializeComponent();

            // ملء قائمة فلتر الحالة
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(PurchaseInvoiceStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;

            LoadPurchases();
        }

        private void LoadPurchases()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.Purchases.Include(p => p.Supplier).AsQueryable();

                    // تطبيق فلتر البحث
                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(p => p.InvoiceNumber.ToLower().Contains(searchText) || p.Supplier.Name.ToLower().Contains(searchText));
                    }
                    // تطبيق فلتر الحالة
                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is PurchaseInvoiceStatus status)
                    {
                        query = query.Where(p => p.Status == status);
                    }

                    // جلب البيانات وتحويلها إلى ViewModel
                    var purchases = query.OrderByDescending(p => p.PurchaseDate).ToList();
                    var viewModels = purchases.Select(p => new PurchaseViewModel
                    {
                        Id = p.Id,
                        InvoiceNumber = p.InvoiceNumber,
                        Supplier = p.Supplier,
                        PurchaseDate = p.PurchaseDate,
                        DueDate = p.DueDate,
                        TotalAmount = p.TotalAmount,
                        AmountPaid = p.AmountPaid,
                        Status = p.Status
                    }).ToList();

                    PurchasesDataGrid.ItemsSource = viewModels;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل فواتير المشتريات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // دالة لمعالجة تغيير الفلاتر
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) { LoadPurchases(); }
        }

        // دالة لمعالجة البحث عند الضغط على Enter
        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { LoadPurchases(); }
        }

        private void AddPurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditPurchaseInvoiceWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadPurchases();
            }
        }

        // دالة لفتح نافذة تسجيل دفعة
        private void RecordPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseViewModel selectedInvoice)
            {
                var paymentWindow = new RecordPurchasePaymentWindow(selectedInvoice.Id);
                if (paymentWindow.ShowDialog() == true)
                {
                    LoadPurchases();
                }
            }
        }

        // --- بداية التحديث: تفعيل زر إنشاء إشعار مدين ---
        private void CreateDebitNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseViewModel selectedInvoice)
            {
                var returnWindow = new AddPurchaseReturnWindow(selectedInvoice.Id);
                if (returnWindow.ShowDialog() == true)
                {
                    LoadPurchases();
                }
            }
        }
        // --- نهاية التحديث ---
    }
}