// UI/Views/SupplierStatementWindow.xaml.cs
// *** تحديث شامل: عرض جميع الحركات المالية للمورد ***
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace GoodMorningFactory.UI.Views
{
    public partial class SupplierStatementWindow : Window
    {
        private readonly int _supplierId;

        public SupplierStatementWindow(int supplierId)
        {
            InitializeComponent();
            _supplierId = supplierId;
            LoadStatement();
        }

        private void LoadStatement()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var supplier = db.Suppliers.Find(_supplierId);
                    if (supplier == null) { this.Close(); return; }

                    SupplierNameTextBlock.Text = $"كشف حساب المورد: {supplier.Name}";

                    // 1. جلب الفواتير (حركة دائنة للمورد)
                    var purchases = db.Purchases
                        .Where(p => p.SupplierId == _supplierId)
                        .Select(p => new { p.PurchaseDate, Type = "فاتورة شراء", Ref = p.InvoiceNumber, Debit = 0m, Credit = p.TotalAmount })
                        .ToList();

                    // 2. جلب المرتجعات (حركة مدينة للمورد)
                    var returns = db.PurchaseReturns
                        .Where(pr => pr.Purchase.SupplierId == _supplierId)
                        .Select(pr => new { ReturnDate = pr.ReturnDate, Type = "مرتجع مشتريات", Ref = pr.ReturnNumber, Debit = pr.TotalReturnValue, Credit = 0m })
                        .ToList();

                    // 3. جلب المدفوعات (حركة مدينة للمورد)
                    var payments = db.JournalVouchers
                        .Where(jv => jv.Description.Contains($"سداد دفعة للمورد: {supplier.Name}"))
                        .Select(jv => new { Date = jv.VoucherDate, Type = "دفعة مسددة", Ref = jv.VoucherNumber, Debit = jv.TotalDebit, Credit = 0m })
                        .ToList();

                    var allTransactions = purchases.Select(p => new { Date = p.PurchaseDate, p.Type, p.Ref, p.Debit, p.Credit })
                                                   .Union(returns.Select(r => new { Date = r.ReturnDate, r.Type, r.Ref, r.Debit, r.Credit }))
                                                   .Union(payments.Select(p => new { p.Date, p.Type, p.Ref, p.Debit, p.Credit }))
                                                .OrderBy(t => t.Date)
                                                .ToList();

                    var statementItems = new List<SupplierStatementItemViewModel>();
                    decimal currentBalance = 0;
                    foreach (var trans in allTransactions)
                    {
                        currentBalance += trans.Credit - trans.Debit; // الرصيد دائن للمورد
                        statementItems.Add(new SupplierStatementItemViewModel
                        {
                            Date = trans.Date,
                            TransactionType = trans.Type,
                            ReferenceNumber = trans.Ref,
                            Debit = trans.Debit,
                            Credit = trans.Credit,
                            Balance = currentBalance
                        });
                    }

                    StatementDataGrid.ItemsSource = statementItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل كشف الحساب: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
