using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class DepartmentsView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalItems = 0;

        public DepartmentsView()
        {
            InitializeComponent();
            LoadDepartments();
        }

        private void LoadDepartments()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.Departments.OrderBy(d => d.Name);
                    _totalItems = query.Count();
                    var departments = query.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
                    DepartmentsDataGrid.ItemsSource = departments;
                }
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الأقسام: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages == 0) totalPages = 1;
            var pageInfoTextBlock = this.FindName("PageInfoTextBlock") as TextBlock;
            if (pageInfoTextBlock != null)
                pageInfoTextBlock.Text = $"الصفحة {_currentPage} من {totalPages} (إجمالي السجلات: {_totalItems})";
            var prevBtn = this.FindName("PreviousPageButton") as Button;
            var nextBtn = this.FindName("NextPageButton") as Button;
            if (prevBtn != null) prevBtn.IsEnabled = _currentPage > 1;
            if (nextBtn != null) nextBtn.IsEnabled = _currentPage < totalPages;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddEditDepartmentWindow();
            if (win.ShowDialog() == true)
                LoadDepartments();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var dept = button?.DataContext as Department;
            if (dept == null)
            {
                MessageBox.Show("يرجى اختيار قسم لتعديله", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var win = new AddEditDepartmentWindow(dept.Id);
            if (win.ShowDialog() == true)
                LoadDepartments();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var dept = button?.DataContext as Department;
            if (dept == null)
            {
                MessageBox.Show("يرجى اختيار قسم لحذفه", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show($"هل أنت متأكد من حذف القسم '{dept.Name}'؟",
                                       "تأكيد الحذف",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var departmentToDelete = db.Departments.Find(dept.Id);
                        if (departmentToDelete != null)
                        {
                            // تحقق من وجود مستخدمين مرتبطين بالقسم
                            bool hasUsers = db.Users.Any(u => u.DepartmentId == dept.Id);
                            if (hasUsers)
                            {
                                MessageBox.Show("لا يمكن حذف القسم لوجود مستخدمين مرتبطين به. يرجى نقل أو حذف المستخدمين أولاً.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            db.Departments.Remove(departmentToDelete);
                            db.SaveChanges();
                            MessageBox.Show("تم حذف القسم بنجاح", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    LoadDepartments();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadDepartments();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (_currentPage < totalPages)
            {
                _currentPage++;
                LoadDepartments();
            }
        }
    }
}