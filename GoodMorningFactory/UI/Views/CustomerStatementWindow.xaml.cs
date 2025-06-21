// UI/Views/CustomerStatementWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة كشف حساب العميل ***
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class CustomerStatementWindow : Window
    {
        private readonly int _customerId;

        public CustomerStatementWindow(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            LoadStatement();
        }

        private void LoadStatement()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var customer = db.Customers.Find(_customerId);
                    if (customer == null) { this.Close(); return; }

                    CustomerNameTextBlock.Text = $"كشف حساب العميل: {customer.CustomerName}";

                    // جلب جميع الحركات المتعلقة بالعميل
                    var sales = db.Sales
                        .Where(s => s.SalesOrder.CustomerId == _customerId)
                        .Select(s => new { Date = s.SaleDate, Type = "فاتورة", Ref = s.InvoiceNumber, Debit = s.TotalAmount, Credit = s.AmountPaid })
                        .ToList();

                    var returns = db.SalesReturns
                        .Where(sr => sr.Sale.SalesOrder.CustomerId == _customerId)
                        .Select(sr => new { Date = sr.ReturnDate, Type = "مرتجع", Ref = sr.ReturnNumber, Debit = 0m, Credit = sr.TotalReturnValue })
                        .ToList();

                    var transactions = sales.Select(s => new { s.Date, s.Type, s.Ref, Debit = s.Debit, Credit = s.Credit })
                                           .Union(returns.Select(r => new { r.Date, r.Type, r.Ref, r.Debit, r.Credit }))
                                           .OrderBy(t => t.Date)
                                           .ToList();

                    var statementItems = new List<CustomerStatementItemViewModel>();
                    decimal currentBalance = 0;
                    foreach (var trans in transactions)
                    {
                        currentBalance += trans.Debit - trans.Credit;
                        statementItems.Add(new CustomerStatementItemViewModel
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