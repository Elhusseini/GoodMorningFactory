// UI/Views/SalesReturnsView.xaml.cs
// الكود الخلفي لواجهة مرتجعات المبيعات
using GoodMorningFactory.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class SalesReturnsView : UserControl
    {
        public SalesReturnsView()
        {
            InitializeComponent();
            LoadReturns();
        }

        public void LoadReturns()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    ReturnsDataGrid.ItemsSource = db.SalesReturns
                        .Include(sr => sr.Sale.SalesOrder.Customer)
                        .OrderByDescending(sr => sr.ReturnDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المرتجعات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}