// UI/Views/RecordPurchasePaymentWindow.xaml.cs
// *** تحديث شامل: إضافة إنشاء قيد محاسبي تلقائي عند تسجيل دفعة لمورد ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore; // Required for Include
using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class RecordPurchasePaymentWindow : Window
    {
        private readonly int _purchaseId;
        private decimal _balanceDue;

        public RecordPurchasePaymentWindow(int purchaseId)
        {
            InitializeComponent();
            _purchaseId = purchaseId;
            LoadInvoiceData();
        }

        private void LoadInvoiceData()
        {
            using (var db = new DatabaseContext())
            {
                var purchase = db.Purchases.Find(_purchaseId);
                if (purchase != null)
                {
                    _balanceDue = purchase.TotalAmount - purchase.AmountPaid;
                    InvoiceNumberTextBlock.Text = purchase.InvoiceNumber;

                    // تنسيق المبالغ مع رمز العملة
                    TotalAmountTextBlock.Text = $"{purchase.TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
                    PreviouslyPaidTextBlock.Text = $"{purchase.AmountPaid:N2} {AppSettings.DefaultCurrencySymbol}";
                    BalanceDueTextBlock.Text = $"{_balanceDue:N2} {AppSettings.DefaultCurrencySymbol}";
                    AmountToPayTextBox.Text = _balanceDue.ToString("N2");
                }
            }
        }

        private void ConfirmPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountToPayTextBox.Text, out decimal amountToPay) || amountToPay <= 0)
            {
                MessageBox.Show("يرجى إدخال مبلغ صحيح للدفع.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (amountToPay > _balanceDue)
            {
                MessageBox.Show("المبلغ المدفوع أكبر من الرصيد المستحق.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var purchase = db.Purchases.Include(p => p.Supplier).FirstOrDefault(p => p.Id == _purchaseId);
                    if (purchase != null)
                    {
                        purchase.AmountPaid += amountToPay;

                        if (purchase.AmountPaid >= purchase.TotalAmount)
                        {
                            purchase.Status = PurchaseInvoiceStatus.FullyPaid;
                        }
                        else
                        {
                            purchase.Status = PurchaseInvoiceStatus.PartiallyPaid;
                        }

                        // --- بداية التحديث: إنشاء القيد المحاسبي ---
                        var companyInfo = db.CompanyInfos.FirstOrDefault();
                        if (companyInfo?.DefaultCashAccountId == null || companyInfo?.DefaultAccountsPayableAccountId == null)
                        {
                            throw new Exception("لا يمكن إنشاء القيد المحاسبي. يرجى التأكد من تحديد حساب 'النقدية/البنك' و 'الذمم الدائنة' الافتراضي في شاشة الإعدادات.");
                        }

                        var journalVoucher = new JournalVoucher
                        {
                            VoucherNumber = $"PAY-{purchase.InvoiceNumber}-{DateTime.Now:HHmmss}",
                            VoucherDate = DateTime.Today,
                            Description = $"سداد دفعة للمورد: {purchase.Supplier.Name} للفاتورة رقم: {purchase.InvoiceNumber}",
                            TotalDebit = amountToPay,
                            TotalCredit = amountToPay,
                            Status = VoucherStatus.Posted
                        };

                        // مدين: حساب الذمم الدائنة (لتقليل الالتزام)
                        journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsPayableAccountId.Value, Debit = amountToPay, Credit = 0 });
                        // دائن: حساب النقدية/البنك (لتقليل رصيد النقدية)
                        journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultCashAccountId.Value, Debit = 0, Credit = amountToPay });

                        db.JournalVouchers.Add(journalVoucher);
                        // --- نهاية التحديث ---

                        db.SaveChanges();
                        transaction.Commit();
                        MessageBox.Show("تم تسجيل الدفعة والقيد المحاسبي بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت عملية حفظ الدفعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
