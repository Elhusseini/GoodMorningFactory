// UI/Views/SalesReturnsView.xaml.cs
// *** تحديث: تم تطوير الكود الخلفي بالكامل ليشمل البحث والفلاتر والطباعة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
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
    public partial class SalesReturnsView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

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
                    var query = db.SalesReturns
                        .Include(sr => sr.Sale.Customer) // تم تغيير الربط ليكون مباشرة مع العميل
                        .AsQueryable();

                    // تطبيق فلتر البحث
                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(sr => sr.ReturnNumber.ToLower().Contains(searchText) ||
                                                 sr.Sale.InvoiceNumber.ToLower().Contains(searchText) ||
                                                 sr.Sale.Customer.CustomerName.ToLower().Contains(searchText));
                    }

                    // تطبيق فلتر التاريخ
                    if (FromDatePicker.SelectedDate.HasValue)
                    {
                        query = query.Where(sr => sr.ReturnDate >= FromDatePicker.SelectedDate.Value);
                    }
                    if (ToDatePicker.SelectedDate.HasValue)
                    {
                        query = query.Where(sr => sr.ReturnDate <= ToDatePicker.SelectedDate.Value.AddDays(1).AddTicks(-1));
                    }

                    _totalItems = query.Count();

                    ReturnsDataGrid.ItemsSource = query
                        .OrderByDescending(sr => sr.ReturnDate)
                        .Skip((_currentPage - 1) * _pageSize)
                        .Take(_pageSize)
                        .ToList();

                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المرتجعات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1) { _currentPage--; LoadReturns(); }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (_currentPage < totalPages) { _currentPage++; LoadReturns(); }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded) { _currentPage = 1; LoadReturns(); }
        }

        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { _currentPage = 1; LoadReturns(); }
        }

        private void PrintCreditNote_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesReturn returnToPrint)
            {
                string tempXpsFile = null;
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var salesReturn = db.SalesReturns
                                          .Include(sr => sr.Sale.Customer)
                                          .Include(sr => sr.SalesReturnItems).ThenInclude(sri => sri.Product)
                                          .FirstOrDefault(sr => sr.Id == returnToPrint.Id);

                        if (salesReturn == null) { MessageBox.Show("لم يتم العثور على المرتجع."); return; }

                        var companyInfo = db.CompanyInfos.FirstOrDefault();
                        string currencySymbol = Core.Services.AppSettings.DefaultCurrencySymbol;

                        var resourceUri = new Uri("/UI/Views/CreditNoteTemplate.xaml", UriKind.Relative);
                        var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                        var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["CreditNote"])) as FlowDocument;

                        // تعبئة بيانات الشركة
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

                        // تعبئة بيانات الإشعار
                        (flowDocument.FindName("CreditNoteNumberRun") as Run).Text = salesReturn.ReturnNumber;
                        (flowDocument.FindName("CreditNoteDateRun") as Run).Text = salesReturn.ReturnDate.ToString("yyyy/MM/dd");
                        (flowDocument.FindName("OriginalInvoiceRun") as Run).Text = salesReturn.Sale.InvoiceNumber;
                        (flowDocument.FindName("CustomerNameRun") as Run).Text = salesReturn.Sale.Customer.CustomerName;
                        (flowDocument.FindName("CustomerAddressRun") as Run).Text = salesReturn.Sale.Customer.BillingAddress;
                        (flowDocument.FindName("TotalValueRun") as Run).Text = $"{salesReturn.TotalReturnValue:N2} {currencySymbol}";

                        // تعبئة الأصناف المرتجعة
                        var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                        foreach (var item in salesReturn.SalesReturnItems)
                        {
                            // نحتاج سعر الوحدة من الفاتورة الأصلية
                            var originalSaleItem = db.SaleItems.FirstOrDefault(si => si.SaleId == salesReturn.SaleId && si.ProductId == item.ProductId);
                            decimal unitPrice = originalSaleItem?.UnitPrice ?? 0;
                            decimal subtotal = item.Quantity * unitPrice;

                            var row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.ProductCode))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { TextAlignment = TextAlignment.Center }));
                            row.Cells.Add(new TableCell(new Paragraph(new Run($"{unitPrice:N2} {currencySymbol}")) { TextAlignment = TextAlignment.Center }));
                            row.Cells.Add(new TableCell(new Paragraph(new Run($"{subtotal:N2} {currencySymbol}")) { TextAlignment = TextAlignment.Center }));
                            itemsTableGroup.Rows.Add(row);
                        }

                        // إنشاء وعرض المستند
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
                    MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}\n\nتأكد من وجود ملف القالب 'CreditNoteTemplate.xaml'.", "خطأ");
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
