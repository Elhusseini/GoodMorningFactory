// UI/Views/ShipmentsView.xaml.cs
// *** تحديث: تم تعديل منطق الطباعة ليتوافق مع القالب الجديد ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;

namespace GoodMorningFactory.UI.Views
{
    public partial class ShipmentsView : UserControl
    {
        public ShipmentsView()
        {
            InitializeComponent();
            LoadFilters();
            LoadShipments();
        }

        private void LoadFilters()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(ShipmentStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;
        }

        public void LoadShipments()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.Shipments
                        .Include(s => s.SalesOrder)
                        .ThenInclude(so => so.Customer)
                        .AsQueryable();

                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(s => s.ShipmentNumber.ToLower().Contains(searchText) ||
                                                 s.SalesOrder.SalesOrderNumber.ToLower().Contains(searchText) ||
                                                 s.SalesOrder.Customer.CustomerName.ToLower().Contains(searchText));
                    }

                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is ShipmentStatus status)
                    {
                        query = query.Where(s => s.Status == status);
                    }

                    if (FromDatePicker.SelectedDate.HasValue)
                    {
                        query = query.Where(s => s.ShipmentDate >= FromDatePicker.SelectedDate.Value);
                    }
                    if (ToDatePicker.SelectedDate.HasValue)
                    {
                        query = query.Where(s => s.ShipmentDate <= ToDatePicker.SelectedDate.Value.AddDays(1).AddTicks(-1));
                    }

                    ShipmentsDataGrid.ItemsSource = query.OrderByDescending(s => s.ShipmentDate).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الشحنات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded) { LoadShipments(); }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) { LoadShipments(); }
        }

        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { LoadShipments(); }
        }

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

        private void PrintPackingSlip_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Shipment shipmentToPrint)
            {
                string tempXpsFile = null;
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var shipment = db.Shipments
                                         .Include(s => s.SalesOrder.Customer)
                                         .Include(s => s.ShipmentItems).ThenInclude(si => si.Product)
                                         .FirstOrDefault(s => s.Id == shipmentToPrint.Id);

                        if (shipment == null) { MessageBox.Show("لم يتم العثور على الشحنة."); return; }

                        var companyInfo = db.CompanyInfos.FirstOrDefault();

                        var resourceUri = new Uri("/UI/Views/PackingSlipTemplate.xaml", UriKind.Relative);
                        var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                        var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["PackingSlip"])) as FlowDocument;

                        (flowDocument.FindName("CompanyNameRun") as Run).Text = companyInfo?.CompanyName ?? "اسم الشركة";
                        (flowDocument.FindName("CompanyAddressRun") as Run).Text = companyInfo?.Address ?? "";
                        if (companyInfo?.Logo != null)
                        {
                            var logoImage = new BitmapImage();
                            using (var stream = new MemoryStream(companyInfo.Logo))
                            {
                                logoImage.BeginInit();
                                logoImage.StreamSource = stream;
                                logoImage.CacheOption = BitmapCacheOption.OnLoad;
                                logoImage.EndInit();
                                logoImage.Freeze();
                            }
                            (flowDocument.FindName("CompanyLogoImage") as Image).Source = logoImage;
                        }

                        (flowDocument.FindName("ShipmentNumberRun") as Run).Text = shipment.ShipmentNumber;
                        (flowDocument.FindName("ShipmentDateRun") as Run).Text = shipment.ShipmentDate.ToString("yyyy/MM/dd");
                        (flowDocument.FindName("OrderNumberRun") as Run).Text = shipment.SalesOrder.SalesOrderNumber;
                        (flowDocument.FindName("CustomerNameRun") as Run).Text = shipment.SalesOrder.Customer.CustomerName;
                        (flowDocument.FindName("ShippingAddressRun") as Run).Text = shipment.SalesOrder.Customer.ShippingAddress ?? shipment.SalesOrder.Customer.BillingAddress;

                        var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                        int counter = 1;
                        foreach (var item in shipment.ShipmentItems)
                        {
                            var row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run(counter.ToString())) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1) }));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.ProductCode)) { Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1) }));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name)) { Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1) }));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1) }));
                            itemsTableGroup.Rows.Add(row);
                            counter++;
                        }

                        tempXpsFile = Path.GetTempFileName();
                        using (var xpsDoc = new XpsDocument(tempXpsFile, FileAccess.ReadWrite))
                        {
                            var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                            var xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                            xpsWriter.Write(paginator);
                            var previewWindow = new PrintPreviewWindow(xpsDoc.GetFixedDocumentSequence(), tempXpsFile);
                            previewWindow.ShowDialog();
                        }
                        tempXpsFile = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}\n\nتأكد من وجود ملف القالب 'PackingSlipTemplate.xaml'.", "خطأ");
                }
                finally
                {
                    if (tempXpsFile != null && File.Exists(tempXpsFile))
                    {
                        try { File.Delete(tempXpsFile); } catch { }
                    }
                }
            }
        }
    }
}
