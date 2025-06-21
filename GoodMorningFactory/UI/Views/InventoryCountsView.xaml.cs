// GoodMorning/UI/Views/InventoryCountsView.xaml.cs
// *** تحديث: تم تفعيل أزرار الإضافة والتعديل ***
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

namespace GoodMorningFactory.UI.Views
{
    public partial class InventoryCountsView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalItems = 0;

        public InventoryCountsView()
        {
            InitializeComponent();
            LoadFilters();
            LoadCounts();
        }

        private void LoadFilters()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(InventoryCountStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void LoadCounts()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.InventoryCounts
                                  .Include(ic => ic.Warehouse)
                                  .Include(ic => ic.ResponsibleUser)
                                  .AsQueryable();

                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(ic => ic.CountReferenceNumber.ToLower().Contains(searchText) || ic.Warehouse.Name.ToLower().Contains(searchText));
                    }

                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is InventoryCountStatus status)
                    {
                        query = query.Where(ic => ic.Status == status);
                    }

                    _totalItems = query.Count();

                    var counts = query.OrderByDescending(ic => ic.CountDate)
                                      .Skip((_currentPage - 1) * _pageSize)
                                      .Take(_pageSize)
                                      .ToList();

                    var viewModels = counts.Select(ic => new InventoryCountViewModel
                    {
                        Id = ic.Id,
                        CountReferenceNumber = ic.CountReferenceNumber,
                        CountDate = ic.CountDate,
                        WarehouseName = ic.Warehouse.Name,
                        Status = ic.Status,
                        ResponsibleUser = ic.ResponsibleUser?.Username ?? "غير محدد"
                    }).ToList();

                    CountsDataGrid.ItemsSource = viewModels;
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل أوامر الجرد: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => LoadCounts();
        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadCounts(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage < (int)Math.Ceiling((double)_totalItems / _pageSize)) { _currentPage++; LoadCounts(); } }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadCounts(); } }
        private void Filter_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { _currentPage = 1; LoadCounts(); } }

        private void AddCountButton_Click(object sender, RoutedEventArgs e)
        {
            // --- تفعيل الكود ---
            var addWindow = new AddEditInventoryCountWindow();
            if (addWindow.ShowDialog() == true) { LoadCounts(); }
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is InventoryCountViewModel vm)
            {
                // --- تفعيل الكود ---
                var editWindow = new AddEditInventoryCountWindow(vm.Id);
                if (editWindow.ShowDialog() == true) { LoadCounts(); }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is InventoryCountViewModel vm)
            {
                if (vm.Status == InventoryCountStatus.Completed || vm.Status == InventoryCountStatus.Cancelled)
                {
                    MessageBox.Show("لا يمكن إلغاء أمر جرد مكتمل أو ملغي بالفعل.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"هل أنت متأكد من إلغاء أمر الجرد رقم '{vm.CountReferenceNumber}'؟", "تأكيد الإلغاء", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            var count = db.InventoryCounts.Find(vm.Id);
                            if (count != null)
                            {
                                count.Status = InventoryCountStatus.Cancelled;
                                db.SaveChanges();
                                LoadCounts();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل إلغاء أمر الجرد: {ex.Message}", "خطأ");
                    }
                }
            }
        }
    }
}
