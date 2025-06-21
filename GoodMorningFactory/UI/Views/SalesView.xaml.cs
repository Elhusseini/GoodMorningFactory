// UI/Views/SalesView.xaml.cs
// *** تحديث: تم تعديل منطق الطباعة ليستخدم أسماء العملة الافتراضية بشكل ديناميكي ***
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
using GoodMorningFactory.UI.ViewModels;
using System.Windows.Xps;
using System.Text;
using Microsoft.Win32;
using GoodMorningFactory.Core.Services;

namespace GoodMorningFactory.UI.Views
{
    public partial class SalesView : UserControl
    {
        public SalesView()
        {
            InitializeComponent();
            LoadFilters();
            LoadSales();
        }

        private void LoadFilters()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(InvoiceStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;

            using (var db = new DatabaseContext())
            {
                var customers = new List<object> { new { CustomerName = "الكل", Id = 0 } };
                customers.AddRange(db.Customers.Where(c => c.IsActive).ToList());
                CustomerFilterComboBox.ItemsSource = customers;
                CustomerFilterComboBox.DisplayMemberPath = "CustomerName";
                CustomerFilterComboBox.SelectedValuePath = "Id";
                CustomerFilterComboBox.SelectedIndex = 0;
            }
        }

        private void LoadSales()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.Sales.Include(s => s.Customer).Include(s => s.SalesOrder).AsQueryable();

                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(s => s.InvoiceNumber.ToLower().Contains(searchText) ||
                                                 (s.SalesOrder != null && s.SalesOrder.SalesOrderNumber.ToLower().Contains(searchText)) ||
                                                 (s.Customer != null && s.Customer.CustomerName.ToLower().Contains(searchText)));
                    }

                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is InvoiceStatus status)
                    {
                        query = query.Where(s => s.Status == status);
                    }

                    if (CustomerFilterComboBox.SelectedIndex > 0)
                    {
                        int customerId = (int)CustomerFilterComboBox.SelectedValue;
                        query = query.Where(s => s.CustomerId == customerId);
                    }

                    if (DueDateFromPicker.SelectedDate.HasValue)
                    {
                        query = query.Where(s => s.DueDate >= DueDateFromPicker.SelectedDate.Value);
                    }
                    if (DueDateToPicker.SelectedDate.HasValue)
                    {
                        query = query.Where(s => s.DueDate <= DueDateToPicker.SelectedDate.Value.AddDays(1).AddTicks(-1));
                    }

                    var salesViewModels = query
                        .OrderByDescending(s => s.SaleDate)
                        .Select(s => new SalesViewViewModel
                        {
                            Id = s.Id,
                            InvoiceNumber = s.InvoiceNumber,
                            SalesOrder = s.SalesOrder,
                            CustomerName = s.Customer.CustomerName,
                            SaleDate = s.SaleDate,
                            DueDate = s.DueDate,
                            Status = s.Status,
                            TotalAmount = s.TotalAmount,
                            AmountPaid = s.AmountPaid
                        })
                        .ToList();

                    SalesDataGrid.ItemsSource = salesViewModels;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الفواتير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded) { LoadSales(); }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) { LoadSales(); }
        }

        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { LoadSales(); }
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
            if ((sender as Button)?.DataContext is SalesViewViewModel selectedInvoiceVM)
            {
                PrintInvoice(selectedInvoiceVM);
            }
        }

        private void PrintSelectedInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SalesDataGrid.SelectedItem is SalesViewViewModel selectedInvoiceVM)
            {
                PrintInvoice(selectedInvoiceVM);
            }
            else
            {
                MessageBox.Show("يرجى تحديد فاتورة من الجدول لطباعتها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // --- بداية الإضافة: دالة زر التصدير ---
        private void ExportToCsvButton_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionsService.CanAccess("Reports.Export"))
            {
                MessageBox.Show("ليس لديك صلاحية لتصدير البيانات.", "وصول مرفوض", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dataToExport = SalesDataGrid.ItemsSource as IEnumerable<SalesViewViewModel>;
            if (dataToExport == null || !dataToExport.Any())
            {
                MessageBox.Show("لا توجد بيانات لتصديرها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV (Comma delimited) (*.csv)|*.csv",
                FileName = $"SalesInvoices_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("InvoiceNumber,CustomerName,SaleDate,DueDate,TotalAmount,AmountPaid,BalanceDue,Status");

                    foreach (var invoice in dataToExport)
                    {
                        var line = string.Format("\"{0}\",\"{1}\",{2},{3},{4},{5},{6},\"{7}\"",
                            invoice.InvoiceNumber,
                            invoice.CustomerName,
                            invoice.SaleDate.ToString("yyyy-MM-dd"),
                            invoice.DueDate?.ToString("yyyy-MM-dd"),
                            invoice.TotalAmount,
                            invoice.AmountPaid,
                            invoice.BalanceDue,
                            invoice.Status);
                        sb.AppendLine(line);
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("تم تصدير البيانات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تصدير الملف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        // --- نهاية الإضافة ---

        private void PrintInvoice(SalesViewViewModel invoiceToPrintVM)
        {
            if (invoiceToPrintVM == null) return;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var saleToPrint = db.Sales
                        .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                        .Include(s => s.Customer)
                        .Include(s => s.SalesOrder)
                        .FirstOrDefault(s => s.Id == invoiceToPrintVM.Id);

                    var companyInfo = db.CompanyInfos.FirstOrDefault();

                    if (saleToPrint == null) return;

                    var resourceUri = new Uri("/UI/Views/SalesInvoiceTemplate.xaml", UriKind.Relative);
                    var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                    var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["SalesInvoice"])) as FlowDocument;

                    if (companyInfo != null)
                    {
                        (flowDocument.FindName("CompanyNameTextBlock") as Run).Text = companyInfo.CompanyName ?? "اسم الشركة";
                        (flowDocument.FindName("CompanyAddressTextBlock") as Run).Text = companyInfo.Address;
                        (flowDocument.FindName("CompanyPhoneTextBlock") as Run).Text = companyInfo.PhoneNumber;
                        (flowDocument.FindName("CompanyTaxNumberTextBlock") as Run).Text = companyInfo.TaxNumber;

                        if (companyInfo.Logo != null)
                        {
                            var logoImage = new BitmapImage();
                            using (var stream = new MemoryStream(companyInfo.Logo))
                            {
                                logoImage.BeginInit();
                                logoImage.StreamSource = stream;
                                logoImage.CacheOption = BitmapCacheOption.OnLoad;
                                logoImage.EndInit();
                            }
                            (flowDocument.FindName("CompanyLogoImage") as Image).Source = logoImage;
                        }
                    }

                    string currencySymbol = Core.Services.AppSettings.DefaultCurrencySymbol;
                    (flowDocument.FindName("InvoiceNumberRun") as Run).Text = saleToPrint.InvoiceNumber;
                    (flowDocument.FindName("SaleDateRun") as Run).Text = saleToPrint.SaleDate.ToString("yyyy/MM/dd");
                    (flowDocument.FindName("SalesOrderNumberRun") as Run).Text = saleToPrint.SalesOrder?.SalesOrderNumber ?? string.Empty;
                    (flowDocument.FindName("CustomerNameRun") as Run).Text = saleToPrint.Customer?.CustomerName ?? string.Empty;
                    (flowDocument.FindName("CustomerTaxNumberRun") as Run).Text = saleToPrint.Customer?.TaxNumber ?? string.Empty;
                    (flowDocument.FindName("CustomerAddressRun") as Run).Text = saleToPrint.Customer?.BillingAddress ?? string.Empty;

                    var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                    int rowIndex = 1;
                    foreach (var item in saleToPrint.SaleItems)
                    {
                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(rowIndex.ToString()))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(4), BorderBrush = System.Windows.Media.Brushes.SteelBlue, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(4), BorderBrush = System.Windows.Media.Brushes.SteelBlue, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString()))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(4), BorderBrush = System.Windows.Media.Brushes.SteelBlue, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.UnitPrice:N2} {currencySymbol}"))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(4), BorderBrush = System.Windows.Media.Brushes.SteelBlue, BorderThickness = new Thickness(0, 0, 1, 1) });
                        decimal discount = 0;
                        var discountProp = item.GetType().GetProperty("Discount");
                        if (discountProp != null)
                            discount = Convert.ToDecimal(discountProp.GetValue(item) ?? 0);
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{discount:N2} {currencySymbol}"))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(4), BorderBrush = System.Windows.Media.Brushes.SteelBlue, BorderThickness = new Thickness(0, 0, 1, 1) });
                        decimal total = (item.Quantity * item.UnitPrice) - discount;
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{total:N2} {currencySymbol}"))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(4), BorderBrush = System.Windows.Media.Brushes.SteelBlue, BorderThickness = new Thickness(0, 0, 0, 1) });
                        itemsTableGroup.Rows.Add(row);
                        rowIndex++;
                    }

                    decimal amountDue = saleToPrint.TotalAmount - saleToPrint.AmountPaid;
                    (flowDocument.FindName("TotalAmountRun") as Run).Text = $"{saleToPrint.TotalAmount:N2} {currencySymbol}";
                    (flowDocument.FindName("AmountPaidRun") as Run).Text = $"{saleToPrint.AmountPaid:N2} {currencySymbol}";
                    (flowDocument.FindName("AmountDueRun") as Run).Text = $"{amountDue:N2} {currencySymbol}";

                    // --- بداية التحديث: جلب أسماء العملة ديناميكياً ---
                    string currencyNameAr = "العملة";
                    string fractionalNameAr = "جزء";

                    if (companyInfo?.DefaultCurrencyId != null)
                    {
                        var defaultCurrency = db.Currencies.Find(companyInfo.DefaultCurrencyId.Value);
                        if (defaultCurrency != null)
                        {
                            currencyNameAr = defaultCurrency.CurrencyName_AR ?? defaultCurrency.Name;
                            fractionalNameAr = defaultCurrency.FractionalUnit_AR ?? "جزء";
                        }
                    }
                    (flowDocument.FindName("TotalInWordsRun") as Run).Text = Core.Helpers.TafqeetHelper.ToWords(saleToPrint.TotalAmount, currencyNameAr, fractionalNameAr);
                    // --- نهاية التحديث ---

                    var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    string tempXpsFile = Path.GetTempFileName();
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
                MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) { LoadSales(); }
        }
    }
}
