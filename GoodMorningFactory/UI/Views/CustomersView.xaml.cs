// UI/Views/CustomersView.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text;
using Microsoft.Win32;
using GoodMorningFactory.Core.Services;
using System.IO; // <-- هذا هو السطر الذي تم إضافته لإصلاح الخطأ

namespace GoodMorningFactory.UI.Views
{
    public partial class CustomersView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public CustomersView()
        {
            InitializeComponent();
            StatusFilterComboBox.ItemsSource = new List<object>
            {
                new { Name = "الكل", Value = (bool?)null },
                new { Name = "نشط", Value = (bool?)true },
                new { Name = "غير نشط", Value = (bool?)false }
            };
            StatusFilterComboBox.DisplayMemberPath = "Name";
            StatusFilterComboBox.SelectedValuePath = "Value";
            StatusFilterComboBox.SelectedIndex = 0;
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                string searchText = SearchTextBox.Text.ToLower();
                bool? statusFilter = (bool?)StatusFilterComboBox.SelectedValue;

                using (var db = new DatabaseContext())
                {
                    var query = db.Customers.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(c => c.CustomerName.ToLower().Contains(searchText) || c.CustomerCode.ToLower().Contains(searchText));
                    }
                    if (statusFilter.HasValue)
                    {
                        query = query.Where(c => c.IsActive == statusFilter.Value);
                    }
                    _totalItems = query.Count();

                    var customers = query.OrderBy(c => c.CustomerName).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

                    var customerViewModels = new List<CustomerViewModel>();
                    foreach (var customer in customers)
                    {
                        var customerInvoices = db.Sales.Where(s => s.CustomerId == customer.Id && s.Status != InvoiceStatus.Cancelled).ToList();
                        decimal totalDebit = customerInvoices.Sum(s => s.TotalAmount);
                        decimal totalCredit = db.SalesReturns.Where(sr => sr.Sale.CustomerId == customer.Id).Sum(sr => (decimal?)sr.TotalReturnValue) ?? 0;
                        totalCredit += customerInvoices.Sum(s => s.AmountPaid);

                        customerViewModels.Add(new CustomerViewModel
                        {
                            Id = customer.Id,
                            CustomerCode = customer.CustomerCode,
                            CustomerName = customer.CustomerName,
                            ContactPerson = customer.ContactPerson,
                            PhoneNumber = customer.PhoneNumber,
                            CreditLimit = customer.CreditLimit,
                            IsActive = customer.IsActive,
                            CurrentBalance = totalDebit - totalCredit
                        });
                    }
                    CustomersDataGrid.ItemsSource = customerViewModels;
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل العملاء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is CustomerViewModel customer)
            {
                var statementWindow = new CustomerStatementWindow(customer.Id);
                statementWindow.Show();
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages == 0) totalPages = 1;
            PageInfoTextBlock.Text = $"الصفحة {_currentPage} من {totalPages} (إجمالي السجلات: {_totalItems})";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < totalPages;
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadCustomers(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize); if (_currentPage < totalPages) { _currentPage++; LoadCustomers(); } }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditCustomerWindow(null);
            if (addWindow.ShowDialog() == true) { _currentPage = 1; LoadCustomers(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is CustomerViewModel customerToEdit)
            {
                using (var db = new DatabaseContext())
                {
                    var originalCustomer = db.Customers.Find(customerToEdit.Id);
                    if (originalCustomer != null)
                    {
                        var editWindow = new AddEditCustomerWindow(originalCustomer);
                        if (editWindow.ShowDialog() == true) { LoadCustomers(); }
                    }
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is CustomerViewModel customerToDelete)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف العميل '{customerToDelete.CustomerName}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            var customer = db.Customers.Find(customerToDelete.Id);
                            if (customer != null)
                            {
                                db.Customers.Remove(customer);
                                db.SaveChanges();
                                LoadCustomers();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"لا يمكن حذف هذا العميل لوجود عمليات مرتبطة به.\nالتفاصيل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _currentPage = 1;
                LoadCustomers();
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                _currentPage = 1;
                LoadCustomers();
            }
        }

        private void ExportToCsvButton_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionsService.CanAccess("Reports.Export"))
            {
                MessageBox.Show("ليس لديك صلاحية لتصدير البيانات.", "وصول مرفوض", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dataToExport = CustomersDataGrid.ItemsSource as IEnumerable<CustomerViewModel>;
            if (dataToExport == null || !dataToExport.Any())
            {
                MessageBox.Show("لا توجد بيانات لتصديرها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV (Comma delimited) (*.csv)|*.csv",
                FileName = $"Customers_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("CustomerCode,CustomerName,ContactPerson,PhoneNumber,CreditLimit,CurrentBalance,IsActive");

                    foreach (var customer in dataToExport)
                    {
                        var line = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4},{5},{6}",
                            customer.CustomerCode,
                            customer.CustomerName,
                            customer.ContactPerson,
                            customer.PhoneNumber,
                            customer.CreditLimit,
                            customer.CurrentBalance,
                            customer.IsActive);
                        sb.AppendLine(line);
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("تم تصدير البيانات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تصدير الملف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
