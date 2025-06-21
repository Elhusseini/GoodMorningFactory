// UI/Views/AuditTrailView.xaml.cs
// الكود الخلفي لواجهة عرض سجلات التدقيق الجديدة

using GoodMorningFactory.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class AuditTrailView : UserControl
    {
        public AuditTrailView()
        {
            InitializeComponent();
            LoadUsersFilter();
            LoadLogs();
        }

        // تحميل قائمة المستخدمين لفلترة السجلات
        private void LoadUsersFilter()
        {
            using (var db = new DatabaseContext())
            {
                // إضافة خيار "الكل"
                var users = db.Users.Select(u => u.Username).ToList();
                users.Insert(0, "الكل");
                UserFilterComboBox.ItemsSource = users;
                UserFilterComboBox.SelectedIndex = 0;
            }
        }

        // تحميل السجلات من قاعدة البيانات مع تطبيق الفلاتر
        private void LoadLogs()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.AuditLogs.AsQueryable();

                    // فلترة حسب التاريخ
                    if (FromDatePicker.SelectedDate.HasValue)
                    {
                        query = query.Where(log => log.Timestamp >= FromDatePicker.SelectedDate.Value.Date);
                    }
                    if (ToDatePicker.SelectedDate.HasValue)
                    {
                        var toDate = ToDatePicker.SelectedDate.Value.Date.AddDays(1);
                        query = query.Where(log => log.Timestamp < toDate);
                    }

                    // فلترة حسب المستخدم
                    if (UserFilterComboBox.SelectedIndex > 0)
                    {
                        var selectedUser = UserFilterComboBox.SelectedItem.ToString();
                        query = query.Where(log => log.Username == selectedUser);
                    }

                    // فلترة حسب نص البحث
                    string searchText = SearchTextBox.Text;
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(log => log.EntityName.Contains(searchText) || log.Changes.Contains(searchText));
                    }

                    AuditLogDataGrid.ItemsSource = query.OrderByDescending(log => log.Timestamp).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل السجلات: {ex.Message}", "خطأ");
            }
        }

        // معالج حدث النقر على زر البحث لتطبيق الفلاتر
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }
    }
}

// ملاحظة: يجب إنشاء ملف XAML المرافق (AuditTrailView.xaml)
// والذي سيحتوي على عناصر الواجهة مثل DatePicker و ComboBox و DataGrid.
