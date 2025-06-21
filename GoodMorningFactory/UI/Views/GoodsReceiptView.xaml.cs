// UI/Views/GoodsReceiptView.xaml.cs
// *** الكود الكامل للكود الخلفي لواجهة استلام البضائع مع إضافة منطق الطباعة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace GoodMorningFactory.UI.Views
{
    public partial class GoodsReceiptView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public GoodsReceiptView()
        {
            InitializeComponent();
            LoadGoodsReceipts();
        }

        private void LoadGoodsReceipts()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    _totalItems = db.GoodsReceiptNotes.Count();

                    GrnDataGrid.ItemsSource = db.GoodsReceiptNotes
                        .Include(grn => grn.PurchaseOrder.Supplier)
                        .OrderByDescending(grn => grn.ReceiptDate)
                        .Skip((_currentPage - 1) * _pageSize)
                        .Take(_pageSize)
                        .ToList();

                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل مذكرات الاستلام: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadGoodsReceipts(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage < (int)Math.Ceiling((double)_totalItems / _pageSize)) { _currentPage++; LoadGoodsReceipts(); } }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GoodsReceiptNote grn)
            {
                var detailsWindow = new GoodsReceiptDetailWindow(grn.Id);
                detailsWindow.ShowDialog();
            }
        }

        // --- بداية التحديث: دالة الطباعة ---
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GoodsReceiptNote grnToPrint)
            {
                XpsDocument xpsDoc = null;
                Uri packUri = null;

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var grn = db.GoodsReceiptNotes
                                  .Include(g => g.PurchaseOrder.Supplier)
                                  .Include(g => g.GoodsReceiptNoteItems)
                                  .ThenInclude(i => i.Product)
                                  .FirstOrDefault(g => g.Id == grnToPrint.Id);

                        if (grn == null) return;

                        var resourceUri = new Uri("/UI/Views/GrnPrintTemplate.xaml", UriKind.Relative);
                        var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                        var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["GoodsReceiptNote"])) as FlowDocument;

                        (flowDocument.FindName("GrnNumberRun") as Run).Text = grn.GRNNumber;
                        (flowDocument.FindName("ReceiptDateRun") as Run).Text = grn.ReceiptDate.ToString("d");
                        (flowDocument.FindName("SupplierNameRun") as Run).Text = grn.PurchaseOrder.Supplier.Name;
                        (flowDocument.FindName("PoNumberRun") as Run).Text = grn.PurchaseOrder.PurchaseOrderNumber;

                        var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                        foreach (var item in grn.GoodsReceiptNoteItems)
                        {
                            var row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.ProductCode))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.QuantityReceived.ToString()))));
                            itemsTableGroup.Rows.Add(row);
                        }

                        var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                        var package = Package.Open(new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);
                        packUri = new Uri("pack://temp.xps");
                        PackageStore.AddPackage(packUri, package);
                        xpsDoc = new XpsDocument(package, CompressionOption.NotCompressed, packUri.ToString());
                        var xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                        xpsWriter.Write(paginator);

                        var previewWindow = new PrintPreviewWindow(xpsDoc.GetFixedDocumentSequence());
                        previewWindow.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}", "خطأ");
                }
                finally
                {
                    xpsDoc?.Close();
                    if (packUri != null && PackageStore.GetPackage(packUri) != null) { PackageStore.RemovePackage(packUri); }
            }
        }
    }
        // --- نهاية التحديث ---
    }
}