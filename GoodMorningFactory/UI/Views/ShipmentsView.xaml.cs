// UI/Views/ShipmentsView.xaml.cs
// *** تحديث: تمت إضافة منطق لتعديل الشحنة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class ShipmentsView : UserControl
    {
        public ShipmentsView()
        {
            InitializeComponent();
            LoadShipments();
        }

        public void LoadShipments()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    ShipmentsDataGrid.ItemsSource = db.Shipments
                        .Include(s => s.SalesOrder)
                        .ThenInclude(so => so.Customer)
                        .OrderByDescending(s => s.ShipmentDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الشحنات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // *** دالة جديدة لمعالجة تعديل الشحنة ***
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Shipment shipmentToEdit)
            {
                var editWindow = new EditShipmentWindow(shipmentToEdit.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadShipments();
                }
            }
        }
    }
}