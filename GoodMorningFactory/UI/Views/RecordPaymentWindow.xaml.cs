using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using GoodMorningFactory.Core.Services; // *** إضافة using جديدة ***

namespace GoodMorningFactory.UI.Views
{
    public partial class RecordPaymentWindow : Window
    {
        private readonly int _saleId;
        private decimal _balanceDue;

        public RecordPaymentWindow(int saleId)
        {
            InitializeComponent();
            _saleId = saleId;
            LoadInvoiceData();
            AmountToPayTextBox.PreviewTextInput += ValidateNumberInput;
        }

        private void LoadInvoiceData()
        {
            using (var db = new DatabaseContext())
            {
                var sale = db.Sales.Find(_saleId);
                if (sale != null)
                {
                    _balanceDue = sale.TotalAmount - sale.AmountPaid;
                    string currencySymbol = AppSettings.DefaultCurrencySymbol; // جلب الرمز من الإعدادات

                    InvoiceNumberTextBlock.Text = sale.InvoiceNumber;

                    // --- بداية التعديل: تحديث طريقة عرض العملة ---
                    TotalAmountTextBlock.Text = $"{sale.TotalAmount:N2} {currencySymbol}";
                    PreviouslyPaidTextBlock.Text = $"{sale.AmountPaid:N2} {currencySymbol}";
                    BalanceDueTextBlock.Text = $"{_balanceDue:N2} {currencySymbol}";
                    // --- نهاية التعديل ---

                    AmountToPayTextBox.Text = _balanceDue.ToString("N2").Replace(",", "");
                }
            }
        }

        private void ValidateNumberInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "[0-9.]");
        }

        private void ConfirmPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountToPayTextBox.Text, out decimal amountPaid) || amountPaid <= 0)
            {
                MessageBox.Show("يرجى إدخال مبلغ صحيح للدفع.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (amountPaid > _balanceDue)
            {
                MessageBox.Show("المبلغ المدفوع أكبر من الرصيد المستحق.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var sale = db.Sales.Include(s => s.Customer).FirstOrDefault(s => s.Id == _saleId);
                    if (sale != null)
                    {
                        sale.AmountPaid += amountPaid;
                        UpdateInvoiceStatus(sale);

                        var companyInfo = db.CompanyInfos.FirstOrDefault();
                        if (companyInfo?.DefaultCashAccountId == null || companyInfo?.DefaultAccountsReceivableAccountId == null)
                        {
                            throw new Exception("لا يمكن إنشاء القيد المحاسبي. يرجى التأكد من تحديد حساب 'النقدية/البنك' و 'الذمم المدينة' الافتراضي في شاشة الإعدادات.");
                        }

                        var journalVoucher = new JournalVoucher
                        {
                            VoucherNumber = $"RCP-{sale.InvoiceNumber}-{DateTime.Now:HHmmss}",
                            VoucherDate = DateTime.Today,
                            Description = $"تحصيل دفعة من العميل: {sale.Customer.CustomerName} للفاتورة رقم: {sale.InvoiceNumber}",
                            TotalDebit = amountPaid,
                            TotalCredit = amountPaid,
                            Status = VoucherStatus.Posted
                        };

                        journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultCashAccountId.Value, Debit = amountPaid, Credit = 0 });
                        journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsReceivableAccountId.Value, Debit = 0, Credit = amountPaid });

                        db.JournalVouchers.Add(journalVoucher);
                        db.SaveChanges();
                        transaction.Commit();
                        MessageBox.Show("تم تسجيل الدفعة والقيد المحاسبي بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت عملية حفظ الدفعة: {ex.Message}", "خطأ");
                }
            }
        }

        private void UpdateInvoiceStatus(Sale sale)
        {
            if (sale.AmountPaid >= sale.TotalAmount)
            {
                sale.Status = InvoiceStatus.Paid;
            }
            else if (sale.AmountPaid > 0)
            {
                sale.Status = InvoiceStatus.PartiallyPaid;
            }
        }
    }
}
