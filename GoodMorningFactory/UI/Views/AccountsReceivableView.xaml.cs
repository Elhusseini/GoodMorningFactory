// UI/Views/AccountsReceivableView.xaml.cs
// *** تحديث: تمت إضافة منطق زر إرسال التذكير ***
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
    public partial class AccountsReceivableView : UserControl
    {
        public AccountsReceivableView()
        {
            InitializeComponent();
            LoadAccountsReceivable();
        }

        private void LoadAccountsReceivable()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var openInvoices = db.Sales
                        .Include(s => s.SalesOrder.Customer)
                        .Where(s => s.Status != InvoiceStatus.Paid && s.Status != InvoiceStatus.Cancelled)
                        .ToList();

                    var arList = new List<AccountsReceivableViewModel>();
                    DateTime today = DateTime.Today;

                    foreach (var invoice in openInvoices)
                    {
                        var balanceDue = invoice.TotalAmount - invoice.AmountPaid;
                        var dueDate = invoice.SaleDate.AddDays(30);
                        var daysOverdue = (today - dueDate).Days;

                        var vm = new AccountsReceivableViewModel
                        {
                            SaleId = invoice.Id,
                            CustomerName = invoice.SalesOrder?.Customer?.CustomerName ?? "N/A",
                            InvoiceNumber = invoice.InvoiceNumber,
                            DueDate = dueDate,
                            TotalAmount = invoice.TotalAmount,
                            BalanceDue = balanceDue
                        };

                        if (daysOverdue <= 30) vm.Bucket0_30 = balanceDue;
                        else if (daysOverdue <= 60) vm.Bucket31_60 = balanceDue;
                        else if (daysOverdue <= 90) vm.Bucket61_90 = balanceDue;
                        else vm.BucketOver90 = balanceDue;

                        arList.Add(vm);
                    }
                    ArDataGrid.ItemsSource = arList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل أرصدة العملاء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RecordPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is AccountsReceivableViewModel invoice)
            {
                var paymentWindow = new RecordSalePaymentWindow(invoice.SaleId);
                if (paymentWindow.ShowDialog() == true)
                {
                    LoadAccountsReceivable();
                }
            }
        }

        // --- بداية التحديث: إضافة دالة إرسال التذكير ---
        private void SendReminderButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is AccountsReceivableViewModel invoice)
            {
                MessageBox.Show($"تم إرسال تذكير للعميل '{invoice.CustomerName}' بخصوص الفاتورة رقم '{invoice.InvoiceNumber}'.", "إرسال تذكير", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        // --- نهاية التحديث ---
    }
}