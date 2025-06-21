// UI/Views/SalesQuotationsView.xaml.cs
// *** الكود الكامل لواجهة عروض الأسعار مع إضافة الدالة المفقودة ***
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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace GoodMorningFactory.UI.Views
{
    public partial class SalesQuotationsView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public SalesQuotationsView()
        {
            InitializeComponent();

            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(QuotationStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;

            LoadQuotations();
        }

        private void LoadQuotations()
        {
            try
            {
                string searchText = SearchTextBox.Text.ToLower();
                DateTime? fromDate = FromDatePicker.SelectedDate;
                DateTime? toDate = ToDatePicker.SelectedDate?.AddDays(1).AddTicks(-1);

                using (var db = new DatabaseContext())
                {
                    var query = db.SalesQuotations.Include(q => q.Customer).AsQueryable();

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(q => q.QuotationNumber.ToLower().Contains(searchText) || q.Customer.CustomerName.ToLower().Contains(searchText));
                    }
                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is QuotationStatus status)
                    {
                        query = query.Where(q => q.Status == status);
                    }
                    if (fromDate.HasValue)
                    {
                        query = query.Where(q => q.QuotationDate >= fromDate.Value);
                    }
                    if (toDate.HasValue)
                    {
                        query = query.Where(q => q.QuotationDate <= toDate.Value);
                    }

                    _totalItems = query.Count();
                    QuotationsDataGrid.ItemsSource = query.OrderByDescending(q => q.QuotationDate).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل عروض الأسعار: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadQuotations(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage < (int)Math.Ceiling((double)_totalItems / _pageSize)) { _currentPage++; LoadQuotations(); } }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadQuotations(); } }
        private void Filter_Changed(object sender, RoutedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadQuotations(); } }

        // *** بداية الإصلاح: إضافة الدالة المفقودة ***
        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _currentPage = 1;
                LoadQuotations();
            }
        }
        // *** نهاية الإصلاح ***

        private void AddQuotationButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditSalesQuotationWindow();
            if (addWindow.ShowDialog() == true) { LoadQuotations(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesQuotation quotation)
            {
                var editWindow = new AddEditSalesQuotationWindow(quotation.Id);
                if (editWindow.ShowDialog() == true) { LoadQuotations(); }
            }
        }

        private void ConvertToOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesQuotation quotation)
            {
                if (quotation.Status == QuotationStatus.Closed) { MessageBox.Show("تم تحويل عرض السعر هذا إلى أمر بيع من قبل."); return; }
                using (var db = new DatabaseContext()) { var quoteInDb = db.SalesQuotations.Find(quotation.Id); if (quoteInDb != null) { quoteInDb.Status = QuotationStatus.Closed; db.SaveChanges(); } }
                var orderWindow = new AddEditSalesOrderWindow(sourceQuotationId: quotation.Id);
                if (orderWindow.ShowDialog() == true) { LoadQuotations(); }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesQuotation quotationToPrint)
            {
                XpsDocument xpsDoc = null;
                Uri packUri = null;

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var bom = db.SalesQuotations
                                  .Include(b => b.Customer)
                                  .Include(b => b.SalesQuotationItems)
                                  .ThenInclude(i => i.Product)
                                  .FirstOrDefault(b => b.Id == quotationToPrint.Id);

                        if (bom == null) return;

                        var resourceUri = new Uri("/UI/Views/QuotationPrintTemplate.xaml", UriKind.Relative);
                        var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                        var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["SalesQuotation"])) as FlowDocument;

                        (flowDocument.FindName("QuotationNumberRun") as Run).Text = bom.QuotationNumber;
                        (flowDocument.FindName("QuotationDateRun") as Run).Text = bom.QuotationDate.ToString("d");
                        (flowDocument.FindName("CustomerNameRun") as Run).Text = bom.Customer.CustomerName;
                        (flowDocument.FindName("ValidUntilDateRun") as Run).Text = bom.ValidUntilDate.ToString("d");
                        (flowDocument.FindName("TotalAmountRun") as Run).Text = bom.TotalAmount.ToString("C");

                        var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                        foreach (var item in bom.SalesQuotationItems)
                        {
                            var row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.UnitPrice.ToString("C")))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString()))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Discount.ToString("C")))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(((item.UnitPrice * item.Quantity) - item.Discount).ToString("C")))));
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesQuotation quotationToDelete)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف عرض السعر رقم '{quotationToDelete.QuotationNumber}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            var items = db.SalesQuotationItems.Where(i => i.SalesQuotationId == quotationToDelete.Id);
                            db.SalesQuotationItems.RemoveRange(items);
                            db.SalesQuotations.Attach(quotationToDelete);
                            db.SalesQuotations.Remove(quotationToDelete);
                            db.SaveChanges();
                            LoadQuotations();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}");
                    }
                }
            }
        }
    }
}
