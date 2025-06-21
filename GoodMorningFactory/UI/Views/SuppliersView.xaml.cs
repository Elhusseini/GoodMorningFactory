// UI/Views/SuppliersView.xaml.cs
// *** الكود الكامل لواجهة إدارة الموردين مع تحسين حساب الرصيد وتفعيل زر التفاصيل ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class SuppliersView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public SuppliersView()
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

            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            try
            {
                string searchText = SearchTextBox.Text.ToLower();
                bool? statusFilter = (bool?)StatusFilterComboBox.SelectedValue;

                using (var db = new DatabaseContext())
                {
                    var query = db.Suppliers.AsQueryable();

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(s => s.Name.ToLower().Contains(searchText) || s.SupplierCode.ToLower().Contains(searchText) || s.PhoneNumber.Contains(searchText));
                    }
                    if (statusFilter.HasValue)
                    {
                        query = query.Where(s => s.IsActive == statusFilter.Value);
                    }

                    _totalItems = query.Count();

                    var suppliers = query.OrderBy(s => s.Name).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

                    // --- بداية التحديث: تحسين حساب الرصيد الحالي ---
                    var supplierViewModels = new List<SupplierViewModel>();
                    foreach (var supplier in suppliers)
                    {
                        decimal totalPurchases = db.Purchases.Where(p => p.SupplierId == supplier.Id).Sum(p => (decimal?)p.TotalAmount) ?? 0;
                        decimal totalPaid = db.Purchases.Where(p => p.SupplierId == supplier.Id).Sum(p => (decimal?)p.AmountPaid) ?? 0;

                        supplierViewModels.Add(new SupplierViewModel
                        {
                            Id = supplier.Id,
                            Name = supplier.Name,
                            SupplierCode = supplier.SupplierCode,
                            ContactPerson = supplier.ContactPerson,
                            PhoneNumber = supplier.PhoneNumber,
                            Email = supplier.Email,
                            IsActive = supplier.IsActive,
                            CurrentBalance = totalPurchases - totalPaid
                        });
                    }
                    // --- نهاية التحديث ---

                    SuppliersDataGrid.ItemsSource = supplierViewModels;
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الموردين: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadSuppliers(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage < (int)Math.Ceiling((double)_totalItems / _pageSize)) { _currentPage++; LoadSuppliers(); } }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadSuppliers(); } }
        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { _currentPage = 1; LoadSuppliers(); } }

        private void AddSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditSupplierWindow();
            if (addWindow.ShowDialog() == true) { LoadSuppliers(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SupplierViewModel supplierVM)
            {
                using (var db = new DatabaseContext())
                {
                    var supplier = db.Suppliers.Find(supplierVM.Id);
                    var editWindow = new AddEditSupplierWindow(supplier);
                    if (editWindow.ShowDialog() == true) { LoadSuppliers(); }
                }
            }
        }

        // --- بداية التحديث: تفعيل زر عرض التفاصيل ---
        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SupplierViewModel supplier)
            {
                var statementWindow = new SupplierStatementWindow(supplier.Id);
                statementWindow.Show();
            }
        }
        // --- نهاية التحديث ---

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SupplierViewModel supplierToDelete)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف المورد '{supplierToDelete.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            var supplier = db.Suppliers.Find(supplierToDelete.Id);
                            db.Suppliers.Remove(supplier);
                            db.SaveChanges();
                            LoadSuppliers();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"لا يمكن حذف هذا المورد لوجود عمليات مرتبطة به.\nالتفاصيل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}