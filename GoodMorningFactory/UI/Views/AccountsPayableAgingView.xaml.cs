// UI/Views/AccountsPayableAgingView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة أعمار ديون الموردين ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class AccountsPayableAgingView : UserControl
    {
        public AccountsPayableAgingView()
        {
            InitializeComponent();
            LoadPayables();
        }

        private void LoadPayables()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var openInvoices = db.Purchases
                        .Include(p => p.Supplier)
                        .Where(p => p.Status != PurchaseInvoiceStatus.FullyPaid && p.Status != PurchaseInvoiceStatus.Cancelled)
                        .ToList();

                    var apList = new List<AccountsPayableViewModel>();
                    DateTime today = DateTime.Today;

                    foreach (var invoice in openInvoices)
                    {
                        var balanceDue = invoice.TotalAmount - invoice.AmountPaid;
                        DateTime dueDate = invoice.DueDate ?? invoice.PurchaseDate.AddDays(30); // افتراض 30 يوم إذا لم يكن هناك تاريخ استحقاق
                        var daysOverdue = (today - dueDate).Days;

                        var vm = new AccountsPayableViewModel
                        {
                            PurchaseId = invoice.Id,
                            SupplierName = invoice.Supplier.Name,
                            InvoiceNumber = invoice.InvoiceNumber,
                            DueDate = dueDate,
                            TotalAmount = invoice.TotalAmount,
                            BalanceDue = balanceDue
                        };

                        if (daysOverdue <= 30) vm.Bucket0_30 = balanceDue;
                        else if (daysOverdue <= 60) vm.Bucket31_60 = balanceDue;
                        else if (daysOverdue <= 90) vm.Bucket61_90 = balanceDue;
                        else vm.BucketOver90 = balanceDue;

                        apList.Add(vm);
                    }
                    ApDataGrid.ItemsSource = apList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل أرصدة الموردين: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RecordPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is AccountsPayableViewModel invoice)
            {
                var paymentWindow = new RecordPurchasePaymentWindow(invoice.PurchaseId);
                if (paymentWindow.ShowDialog() == true)
                {
                    LoadPayables();
                }
            }
        }
    }
}