// UI/Views/PayrollView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة عرض مسيرات الرواتب ***
using GoodMorningFactory.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class PayrollView : UserControl
    {
        public PayrollView()
        {
            InitializeComponent();
            LoadPayrolls();
        }

        private void LoadPayrolls()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    PayrollsDataGrid.ItemsSource = db.Payrolls.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToList();
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ: {ex.Message}"); }
        }

        private void CreatePayrollButton_Click(object sender, RoutedEventArgs e)
        {
            var processWindow = new ProcessPayrollWindow();
            if (processWindow.ShowDialog() == true)
            {
                LoadPayrolls();
            }
        }
    }
}