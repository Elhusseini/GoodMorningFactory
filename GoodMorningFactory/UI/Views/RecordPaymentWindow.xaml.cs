// UI/Views/RecordPaymentWindow.xaml.cs
// الكود الخلفي لنافذة تسجيل الدفعة
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;

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
        }

        private void LoadInvoiceData()
        {
            using (var db = new DatabaseContext())
            {
                var sale = db.Sales.Find(_saleId);
                if (sale != null)
                {
                    _balanceDue = sale.TotalAmount - sale.AmountPaid;

                    InvoiceNumberTextBlock.Text = sale.InvoiceNumber;
                    TotalAmountTextBlock.Text = sale.TotalAmount.ToString("C", new CultureInfo("ar-KW"));
                    PreviouslyPaidTextBlock.Text = sale.AmountPaid.ToString("C", new CultureInfo("ar-KW"));
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
                    var sale = db.Sales.Find(_saleId);
                    if (sale != null)
                    {
                        sale.AmountPaid += amountToPay;
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