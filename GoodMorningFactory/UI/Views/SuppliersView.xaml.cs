// UI/Views/SuppliersView.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GoodMorningFactory.Core.Services;
using Microsoft.Win32;

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
                        query = query.Where(s => s.Name.ToLower().Contains(searchText) || s.SupplierCode.ToLower().Contains(searchText));
                    }
                    if (statusFilter.HasValue)
                    {
                        query = query.Where(s => s.IsActive == statusFilter.Value);
                    }
                    _totalItems = query.Count();

                    var suppliers = query.OrderBy(s => s.Name).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

                    var supplierViewModels = new List<SupplierViewModel>();
                    foreach (var supplier in suppliers)
                    {
                        decimal totalPurchases = db.Purchases.Where(p => p.SupplierId == supplier.Id).Sum(p => (decimal?)p.TotalAmount) ?? 0;
                        decimal totalPayments = db.Purchases.Where(p => p.SupplierId == supplier.Id).Sum(p => (decimal?)p.AmountPaid) ?? 0;
                        decimal totalReturns = db.PurchaseReturns.Include(pr => pr.Purchase).Where(pr => pr.Purchase.SupplierId == supplier.Id).Sum(pr => (decimal?)pr.TotalReturnValue) ?? 0;

                        supplierViewModels.Add(new SupplierViewModel
                        {
                            Id = supplier.Id,
                            SupplierCode = supplier.SupplierCode,
                            Name = supplier.Name,
                            ContactPerson = supplier.ContactPerson,
                            PhoneNumber = supplier.PhoneNumber,
                            IsActive = supplier.IsActive,
                            CurrentBalance = totalPurchases - totalPayments - totalReturns
                        });
                    }

                    SuppliersDataGrid.ItemsSource = supplierViewModels;
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الموردين: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SupplierViewModel supplier)
            {
                var statementWindow = new SupplierStatementWindow(supplier.Id);
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

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadSuppliers(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize); if (_currentPage < totalPages) { _currentPage++; LoadSuppliers(); } }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadSuppliers(); } }
        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { _currentPage = 1; LoadSuppliers(); } }

        private void AddSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditSupplierWindow();
            if (addWindow.ShowDialog() == true) { _currentPage = 1; LoadSuppliers(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SupplierViewModel supplierVM)
            {
                using (var db = new DatabaseContext())
                {
                    var originalSupplier = db.Suppliers.Find(supplierVM.Id);
                    if (originalSupplier != null)
                    {
                        var editWindow = new AddEditSupplierWindow(originalSupplier);
                        if (editWindow.ShowDialog() == true) { LoadSuppliers(); }
                    }
                }
            }
        }

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
                            if (supplier != null)
                            {
                                db.Suppliers.Remove(supplier);
                                db.SaveChanges();
                                LoadSuppliers();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"لا يمكن حذف هذا المورد لوجود عمليات مرتبطة به.\nالتفاصيل: {ex.InnerException?.Message ?? ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ExportToCsvButton_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionsService.CanAccess("Reports.Export"))
            {
                MessageBox.Show("ليس لديك صلاحية لتصدير البيانات.", "وصول مرفوض", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dataToExport = SuppliersDataGrid.ItemsSource as IEnumerable<SupplierViewModel>;
            if (dataToExport == null || !dataToExport.Any())
            {
                MessageBox.Show("لا توجد بيانات لتصديرها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV (Comma delimited) (*.csv)|*.csv",
                FileName = $"Suppliers_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("SupplierCode,Name,ContactPerson,PhoneNumber,CurrentBalance,IsActive");

                    foreach (var supplier in dataToExport)
                    {
                        var line = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4},{5}",
                            supplier.SupplierCode,
                            supplier.Name,
                            supplier.ContactPerson,
                            supplier.PhoneNumber,
                            supplier.CurrentBalance,
                            supplier.IsActive);
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
