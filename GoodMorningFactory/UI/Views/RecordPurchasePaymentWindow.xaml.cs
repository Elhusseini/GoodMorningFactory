// UI/Views/RecordPurchasePaymentWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة تسجيل دفعة لمورد ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
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
                    TotalAmountTextBlock.Text = purchase.TotalAmount.ToString("C", new CultureInfo("ar-KW"));
                    PreviouslyPaidTextBlock.Text = purchase.AmountPaid.ToString("C", new CultureInfo("ar-KW"));
                    BalanceDueTextBlock.Text = _balanceDue.ToString("C", new CultureInfo("ar-KW"));
                    AmountToPayTextBox.Text = _balanceDue.ToString("N2").Replace(",", "");
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

            try
            {
                using (var db = new DatabaseContext())
                {
                    var purchase = db.Purchases.Find(_purchaseId);
                    if (purchase != null)
                    {
                        purchase.AmountPaid += amountToPay;

                        // تحديث حالة الفاتورة
                        if (purchase.AmountPaid >= purchase.TotalAmount)
                        {
                            purchase.Status = PurchaseInvoiceStatus.FullyPaid;
                        }
                        else
                        {
                            purchase.Status = PurchaseInvoiceStatus.PartiallyPaid;
                        }

                        db.SaveChanges();
                        MessageBox.Show("تم تسجيل الدفعة بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية حفظ الدفعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}