// UI/Views/PurchasesView.xaml.cs
// *** تحديث: تم إصلاح منطق الطباعة ليشمل التفقيط والعملة الافتراضية بشكل صحيح ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
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
using GoodMorningFactory.Core.Services;

namespace GoodMorningFactory.UI.Views
{
    public partial class PurchasesView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public PurchasesView()
        {
            InitializeComponent();

            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(PurchaseInvoiceStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;

            LoadPurchases();
        }

        private void LoadPurchases()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.Purchases.Include(p => p.Supplier).AsQueryable();

                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(p => p.InvoiceNumber.ToLower().Contains(searchText) || p.Supplier.Name.ToLower().Contains(searchText));
                    }
                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is PurchaseInvoiceStatus status)
                    {
                        query = query.Where(p => p.Status == status);
                    }

                    _totalItems = query.Count();
                    var purchases = query.OrderByDescending(p => p.PurchaseDate)
                                         .Skip((_currentPage - 1) * _pageSize)
                                         .Take(_pageSize)
                                         .ToList();

                    var viewModels = purchases.Select(p => new PurchaseViewModel
                    {
                        Id = p.Id,
                        InvoiceNumber = p.InvoiceNumber,
                        Supplier = p.Supplier,
                        PurchaseDate = p.PurchaseDate,
                        DueDate = p.DueDate,
                        TotalAmount = p.TotalAmount,
                        AmountPaid = p.AmountPaid,
                        Status = p.Status
                    }).ToList();

                    PurchasesDataGrid.ItemsSource = viewModels;
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل فواتير المشتريات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages == 0) totalPages = 1;
            PageInfoTextBlock.Text = $"الصفحة {_currentPage} من {totalPages} (إجمالي الفواتير: {_totalItems})";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < totalPages;
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadPurchases();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (_currentPage < totalPages)
            {
                _currentPage++;
                LoadPurchases();
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) { _currentPage = 1; LoadPurchases(); }
        }

        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { _currentPage = 1; LoadPurchases(); }
        }

        private void AddPurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditPurchaseInvoiceWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadPurchases();
            }
        }

        private void RecordPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseViewModel selectedInvoice)
            {
                var paymentWindow = new RecordPurchasePaymentWindow(selectedInvoice.Id);
                if (paymentWindow.ShowDialog() == true)
                {
                    LoadPurchases();
                }
            }
        }

        private void CreateDebitNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseViewModel selectedInvoice)
            {
                var returnWindow = new AddPurchaseReturnWindow(selectedInvoice.Id);
                if (returnWindow.ShowDialog() == true)
                {
                    LoadPurchases();
                }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseViewModel vm)
            {
                string tempXpsFile = null;
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var purchaseToPrint = db.Purchases
                            .Include(p => p.Supplier)
                            .Include(p => p.PurchaseItems)
                            .ThenInclude(pi => pi.Product)
                            .FirstOrDefault(p => p.Id == vm.Id);

                        if (purchaseToPrint == null) return;

                        var companyInfo = db.CompanyInfos.FirstOrDefault();

                        var resourceUri = new Uri("/UI/Views/PurchaseInvoicePrintTemplate.xaml", UriKind.Relative);
                        var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                        var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["PurchaseInvoice"])) as FlowDocument;

                        if (companyInfo != null)
                        {
                            if (flowDocument.FindName("CompanyNameRun") is TextBlock companyName)
                                companyName.Text = companyInfo.CompanyName ?? "اسم الشركة";
                            if (flowDocument.FindName("CompanyAddressRun") is TextBlock companyAddress)
                                companyAddress.Text = companyInfo.Address;
                            if (flowDocument.FindName("CompanyPhoneRun") is TextBlock companyPhone)
                                companyPhone.Text = companyInfo.PhoneNumber;
                            if (companyInfo.Logo != null && flowDocument.FindName("CompanyLogoImage") is Image companyLogo)
                            {
                                var logoImage = new BitmapImage();
                                using (var stream = new MemoryStream(companyInfo.Logo))
                                {
                                    logoImage.BeginInit(); logoImage.StreamSource = stream; logoImage.CacheOption = BitmapCacheOption.OnLoad; logoImage.EndInit(); logoImage.Freeze();
                                }
                                companyLogo.Source = logoImage;
                            }
                        }

                        if (flowDocument.FindName("SupplierNameRun") is TextBlock supplierName)
                            supplierName.Text = purchaseToPrint.Supplier.Name;
                        if (flowDocument.FindName("SupplierAddressRun") is TextBlock supplierAddress)
                            supplierAddress.Text = purchaseToPrint.Supplier.Address ?? "";
                        if (flowDocument.FindName("SupplierTaxNumberRun") is TextBlock supplierTax)
                            supplierTax.Text = $"الرقم الضريبي: {purchaseToPrint.Supplier.TaxNumber ?? "-"}";

                        if (flowDocument.FindName("InvoiceNumberRun") is Run invoiceNumber)
                            invoiceNumber.Text = purchaseToPrint.InvoiceNumber;
                        if (flowDocument.FindName("InvoiceDateRun") is Run invoiceDate)
                            invoiceDate.Text = purchaseToPrint.PurchaseDate.ToString("yyyy/MM/dd");
                        if (flowDocument.FindName("DueDateRun") is Run dueDate)
                            dueDate.Text = purchaseToPrint.DueDate?.ToString("yyyy/MM/dd") ?? "N/A";

                        var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                        foreach (var item in purchaseToPrint.PurchaseItems)
                        {
                            var row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name)) { Padding = new Thickness(5) }));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { Padding = new Thickness(5), TextAlignment = TextAlignment.Center }));
                            row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.UnitPrice:N2}")) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right }));
                            row.Cells.Add(new TableCell(new Paragraph(new Run($"{(item.Quantity * item.UnitPrice):N2}")) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right }));
                            itemsTableGroup.Rows.Add(row);
                        }

                        // --- بداية التحديث: تعبئة الإجمالي والتفقيط مع مراعاة العملة ---
                        if (flowDocument.FindName("SubtotalRun") is TextBlock subtotal)
                            subtotal.Text = $"{purchaseToPrint.TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
                        if (flowDocument.FindName("TaxAmountRun") is TextBlock tax)
                            tax.Text = $"0.00 {AppSettings.DefaultCurrencySymbol}";
                        if (flowDocument.FindName("TotalAmountRun") is TextBlock total)
                            total.Text = $"{purchaseToPrint.TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";

                        // جلب أسماء العملة للتفقيط
                        string currencyNameAr = "العملة";
                        string fractionalNameAr = "جزء";
                        var defaultCurrency = db.Currencies.FirstOrDefault(c => c.IsDefault);
                        if (defaultCurrency != null)
                        {
                            currencyNameAr = defaultCurrency.CurrencyName_AR ?? defaultCurrency.Name;
                            fractionalNameAr = defaultCurrency.FractionalUnit_AR ?? "جزء";
                        }

                        if (flowDocument.FindName("TotalInWordsRun") is TextBlock totalInWords)
                            totalInWords.Text = Core.Helpers.TafqeetHelper.ToWords(purchaseToPrint.TotalAmount, currencyNameAr, fractionalNameAr);
                        // --- نهاية التحديث ---

                        var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                        tempXpsFile = Path.GetTempFileName();
                        using (var xpsDoc = new XpsDocument(tempXpsFile, FileAccess.ReadWrite))
                        {
                            var xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                            xpsWriter.Write(paginator);
                        }
                        using (var xpsDoc = new XpsDocument(tempXpsFile, FileAccess.Read))
                        {
                            var fixedDocSeq = xpsDoc.GetFixedDocumentSequence();
                            var previewWindow = new PrintPreviewWindow(fixedDocSeq, tempXpsFile);
                            previewWindow.ShowDialog();
                        }
                        tempXpsFile = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}", "خطأ");
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
