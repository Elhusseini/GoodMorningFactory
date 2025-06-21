// UI/Views/CustomersView.xaml.cs
// *** الكود الكامل لواجهة إدارة العملاء مع إضافة الدالة المفقودة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
                        query = query.Where(c => c.CustomerName.ToLower().Contains(searchText) || c.CustomerCode.ToLower().Contains(searchText) || c.PhoneNumber.Contains(searchText));
                    }

                    if (statusFilter.HasValue)
                    {
                        query = query.Where(c => c.IsActive == statusFilter.Value);
                    }

                    _totalItems = query.Count();
                    CustomersDataGrid.ItemsSource = query.OrderBy(c => c.CustomerName).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل العملاء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1) { _currentPage--; LoadCustomers(); }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (_currentPage < totalPages) { _currentPage++; LoadCustomers(); }
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditCustomerWindow();
            if (addWindow.ShowDialog() == true)
            {
                _currentPage = 1;
                LoadCustomers();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Customer customerToEdit)
            {
                var editWindow = new AddEditCustomerWindow(customerToEdit);
                if (editWindow.ShowDialog() == true)
                {
                    LoadCustomers();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Customer customerToDelete)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف العميل '{customerToDelete.CustomerName}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            db.Customers.Attach(customerToDelete);
                            db.Customers.Remove(customerToDelete);
                            db.SaveChanges();
                            LoadCustomers();
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

        // *** بداية الإصلاح: إضافة الدالة المفقودة ***
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                _currentPage = 1;
                LoadCustomers();
            }
        }
        // *** نهاية الإصلاح ***
    }
}