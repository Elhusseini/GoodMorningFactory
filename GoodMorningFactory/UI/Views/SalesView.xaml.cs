// UI/Views/SalesView.xaml.cs
// *** الكود الكامل لواجهة فواتير المبيعات مع جميع الوظائف المحدثة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace GoodMorningFactory.UI.Views
{
    // ViewModel لعرض الفواتير في الجدول
    public class SalesViewViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public SalesOrder SalesOrder { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue => TotalAmount - AmountPaid;
    }

    // ViewModel خاص ببيانات الفاتورة للطباعة
    public class SalesInvoicePrintViewModel
    {
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public BitmapImage CompanyLogo { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue => TotalAmount - AmountPaid;
        public ICollection<SaleItemPrintViewModel> SaleItems { get; set; }
    }

    public class SaleItemPrintViewModel
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }

    public partial class SalesView : UserControl
    {
        public SalesView()
        {
            InitializeComponent();
            // ملء قائمة الفلترة
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(InvoiceStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;
            LoadSales();
        }

        private void LoadSales()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.Sales.Include(s => s.SalesOrder).AsQueryable();

                    // تطبيق الفلترة
                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is InvoiceStatus status)
                    {
                        query = query.Where(s => s.Status == status);
                    }

                    SalesDataGrid.ItemsSource = query
                        .OrderByDescending(s => s.SaleDate)
                        .Select(s => new SalesViewViewModel
                        {
                            Id = s.Id,
                            InvoiceNumber = s.InvoiceNumber,
                            SalesOrder = s.SalesOrder,
                            Status = s.Status,
                            TotalAmount = s.TotalAmount,
                            AmountPaid = s.AmountPaid
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الفواتير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewSaleButton_Click(object sender, RoutedEventArgs e)
        {
            var newSaleWindow = new NewDirectSaleWindow();
            if (newSaleWindow.ShowDialog() == true) { LoadSales(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesViewViewModel selectedInvoice)
            {
                // لا يمكن تعديل الفواتير التي تم تأكيدها
                if (selectedInvoice.Status != InvoiceStatus.Draft)
                {
                    MessageBox.Show("لا يمكن تعديل هذه الفاتورة لأنها ليست في حالة 'مسودة'.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var editWindow = new EditSaleWindow(selectedInvoice.Id);
                if (editWindow.ShowDialog() == true) { LoadSales(); }
            }
        }

        private void RecordPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesViewViewModel selectedInvoice)
            {
                var paymentWindow = new RecordPaymentWindow(selectedInvoice.Id);
                if (paymentWindow.ShowDialog() == true) { LoadSales(); }
            }
        }

        private void CreateReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesViewViewModel selectedInvoice)
            {
                var returnWindow = new AddSalesReturnWindow(selectedInvoice.Id);
                if (returnWindow.ShowDialog() == true) { LoadSales(); }
            }
        }

        private void PrintInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SalesDataGrid.SelectedItem is SalesViewViewModel selectedInvoiceVM)
            {
                XpsDocument xpsDoc = null;
                Uri packUri = null;

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var saleToPrint = db.Sales
                            .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                            .FirstOrDefault(s => s.Id == selectedInvoiceVM.Id);

                        var companyInfo = db.CompanyInfos.FirstOrDefault();
                        if (saleToPrint == null) return;

                        var viewModel = new SalesInvoicePrintViewModel
                        {
                            CompanyName = companyInfo?.CompanyName ?? "اسم الشركة",
                            CompanyAddress = companyInfo?.Address,
                            CompanyPhone = companyInfo?.PhoneNumber,
                            InvoiceNumber = saleToPrint.InvoiceNumber,
                            SaleDate = saleToPrint.SaleDate,
                            TotalAmount = saleToPrint.TotalAmount,
                            AmountPaid = saleToPrint.AmountPaid,
                            SaleItems = saleToPrint.SaleItems.Select(si => new SaleItemPrintViewModel
                            {
                                Product = si.Product,
                                Quantity = si.Quantity,
                                UnitPrice = si.UnitPrice
                            }).ToList()
                        };

                        if (companyInfo?.Logo != null)
                        {
                            var image = new BitmapImage();
                            using (var stream = new MemoryStream(companyInfo.Logo))
                            {
                                image.BeginInit(); image.StreamSource = stream; image.CacheOption = BitmapCacheOption.OnLoad; image.EndInit();
                            }
                            viewModel.CompanyLogo = image;
                        }

                        var resourceUri = new Uri("/UI/Views/SalesInvoiceTemplate.xaml", UriKind.Relative);
                        var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                        var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["SalesInvoice"])) as FlowDocument;

                        flowDocument.DataContext = viewModel;

                        var itemsTable = flowDocument.Blocks.OfType<Table>().Skip(2).FirstOrDefault();
                        if (itemsTable != null && itemsTable.RowGroups.Any())
                        {
                            var itemsTableGroup = itemsTable.RowGroups.First();
                            foreach (var item in viewModel.SaleItems)
                            {
                                var row = new TableRow();
                                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))));
                                row.Cells.Add(new TableCell(new Paragraph(new Run(item.UnitPrice.ToString("C")))));
                                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString()))));
                                row.Cells.Add(new TableCell(new Paragraph(new Run((item.Quantity * item.UnitPrice).ToString("C")))));
                                itemsTableGroup.Rows.Add(row);
                            }
                        }

                        var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                        var package = Package.Open(new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);
                        packUri = new Uri("pack://temp.xps");
                        PackageStore.AddPackage(packUri, package);
                        xpsDoc = new XpsDocument(package, CompressionOption.NotCompressed, packUri.ToString());
                        var xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                        xpsWriter.Write(paginator);

                        var fixedDocSeq = xpsDoc.GetFixedDocumentSequence();

                        var previewWindow = new PrintPreviewWindow(fixedDocSeq);
                        previewWindow.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    xpsDoc?.Close();
                    if (packUri != null)
                    {
                        PackageStore.RemovePackage(packUri);
                    }
                }
            }
            else
            {
                MessageBox.Show("يرجى تحديد فاتورة لطباعتها أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSales();
        }
    }
}
