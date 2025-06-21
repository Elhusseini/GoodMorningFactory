// UI/Views/LeaveTypesView.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class LeaveTypesView : UserControl
    {
        public LeaveTypesView()
        {
            InitializeComponent();
            LoadLeaveTypes();
        }

        private void LoadLeaveTypes()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    LeaveTypesDataGrid.ItemsSource = db.LeaveTypes.ToList();
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل أنواع الإجازات: {ex.Message}"); }
        }

        private void AddLeaveType_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditLeaveTypeWindow();
            if (addWindow.ShowDialog() == true) { LoadLeaveTypes(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is LeaveType leaveType)
            {
                var editWindow = new AddEditLeaveTypeWindow(leaveType.Id);
                if (editWindow.ShowDialog() == true) { LoadLeaveTypes(); }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is LeaveType leaveType)
            {
                // (منطق الحذف الآمن يمكن إضافته هنا)
            }
        }
    }
}
