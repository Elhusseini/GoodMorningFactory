// UI/Views/JournalVouchersView.xaml.cs
// *** تحديث: تم إضافة منطق عكس القيد ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class JournalVouchersView : UserControl
    {
        public JournalVouchersView()
        {
            InitializeComponent();
            LoadVouchers();
        }

        private void LoadVouchers()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    VouchersDataGrid.ItemsSource = db.JournalVouchers.OrderByDescending(jv => jv.VoucherDate).ToList();
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل القيود: {ex.Message}", "خطأ"); }
        }

        private void AddVoucherButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditJournalVoucherView();
            if (addWindow.ShowDialog() == true)
            {
                LoadVouchers();
            }
        }

        // --- بداية التحديث: إضافة دالة عكس القيد ---
        private void ReverseVoucher_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is JournalVoucher voucherToReverse)
            {
                var result = MessageBox.Show($"هل أنت متأكد من عكس القيد رقم '{voucherToReverse.VoucherNumber}'؟\nسيتم إنشاء قيد جديد يعكس هذا القيد.", "تأكيد عكس القيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var originalVoucher = db.JournalVouchers.Include(jv => jv.JournalVoucherItems).FirstOrDefault(jv => jv.Id == voucherToReverse.Id);
                        if (originalVoucher == null || originalVoucher.IsReversed)
                        {
                            MessageBox.Show("لا يمكن عكس هذا القيد (قد يكون تم عكسه من قبل).", "عملية غير ممكنة");
                            return;
                        }

                        var reversedVoucher = new JournalVoucher
                        {
                            VoucherNumber = $"REV-{originalVoucher.VoucherNumber}",
                            VoucherDate = DateTime.Today,
                            Description = $"قيد عكسي للقيد رقم: {originalVoucher.VoucherNumber}",
                            TotalDebit = originalVoucher.TotalCredit,
                            TotalCredit = originalVoucher.TotalDebit,
                            Status = VoucherStatus.Posted
                        };

                        foreach (var item in originalVoucher.JournalVoucherItems)
                        {
                            reversedVoucher.JournalVoucherItems.Add(new JournalVoucherItem
                            {
                                AccountId = item.AccountId,
                                Debit = item.Credit, // عكس المدين والدائن
                                Credit = item.Debit   // عكس المدين والدائن
                            });
                        }

                        originalVoucher.IsReversed = true;
                        db.JournalVouchers.Add(reversedVoucher);
                        db.SaveChanges();
                        LoadVouchers();
                    }
                }
                catch (Exception ex) { MessageBox.Show($"فشلت عملية عكس القيد: {ex.Message}", "خطأ"); }
            }
        }
        // --- نهاية التحديث ---
    }
}