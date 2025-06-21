// UI/Views/SupplierStatementWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة كشف حساب المورد ***
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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

                    var purchases = db.Purchases
                        .Where(p => p.SupplierId == _supplierId)
                        .Select(p => new { Date = p.PurchaseDate, Type = "فاتورة شراء", Ref = p.InvoiceNumber, Debit = 0m, Credit = p.TotalAmount })
                        .ToList();

                    var payments = new List<object>(); // سيتم ملؤها عند بناء وحدة المدفوعات

                    var transactions = purchases.Select(p => new { p.Date, p.Type, p.Ref, p.Debit, p.Credit })
                                                .OrderBy(t => t.Date)
                                                .ToList();

                    var statementItems = new List<SupplierStatementItemViewModel>();
                    decimal currentBalance = 0;
                    foreach (var trans in transactions)
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