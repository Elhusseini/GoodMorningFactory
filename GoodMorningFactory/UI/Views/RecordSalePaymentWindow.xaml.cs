// UI/Views/RecordSalePaymentWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة تسجيل دفعة من عميل ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class RecordSalePaymentWindow : Window
    {
        private readonly int _saleId;
        private decimal _balanceDue;

        public RecordSalePaymentWindow(int saleId)
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
                    BalanceDueTextBlock.Text = _balanceDue.ToString("C", new CultureInfo("ar-KW"));
                    AmountToPayTextBox.Text = _balanceDue.ToString("N2").Replace(",", "");
                }
            }
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

            try
            {
                using (var db = new DatabaseContext())
                {
                    var sale = db.Sales.Find(_saleId);
                    if (sale != null)
                    {
                        sale.AmountPaid += amountPaid;
                        if (sale.AmountPaid >= sale.TotalAmount) { sale.Status = InvoiceStatus.Paid; }
                        else { sale.Status = InvoiceStatus.PartiallyPaid; }

                        db.SaveChanges();
                        MessageBox.Show("تم تسجيل الدفعة بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشلت عملية حفظ الدفعة: {ex.Message}", "خطأ"); }
        }
    }
}