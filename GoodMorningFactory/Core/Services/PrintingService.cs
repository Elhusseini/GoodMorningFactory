// GoodMorningFactory/Core/Services/PrintingService.cs
// *** الكود الكامل والنهائي - تم إصلاح منطق تجميع الكميات في الطباعة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Views;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using GoodMorningFactory.Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية موحدة مسؤولة عن جميع عمليات الطباعة في النظام.
    /// </summary>
    public class PrintingService : IPrintingService
    {
        public void PrintVisual(FrameworkElement visual, string description)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var originalTransform = visual.LayoutTransform;
                double scale = CalculateScale(printDialog, visual);
                visual.LayoutTransform = new ScaleTransform(scale, scale);
                var pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                visual.Measure(pageSize);
                visual.Arrange(new Rect(5, 5, pageSize.Width, pageSize.Height));
                printDialog.PrintVisual(visual, description);
                visual.LayoutTransform = originalTransform;
            }
        }

        public async Task PrintPackingSlipAsync(Shipment shipment)
        {
            if (shipment == null)
            {
                MessageBox.Show("لم يتم العثور على بيانات الشحنة للطباعة.");
                return;
            }

            string tempXpsFile = null;
            try
            {
                using (var db = new DatabaseContext())
                {
                    var shipmentToPrint = await db.Shipments
                        .Include(s => s.SalesOrder.Customer)
                        .Include(s => s.ShipmentItems).ThenInclude(si => si.Product)
                        .FirstOrDefaultAsync(s => s.Id == shipment.Id);

                    if (shipmentToPrint == null) return;

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
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

                    (flowDocument.FindName("ShipmentNumberRun") as Run).Text = shipmentToPrint.ShipmentNumber;
                    (flowDocument.FindName("ShipmentDateRun") as Run).Text = shipmentToPrint.ShipmentDate.ToString("yyyy/MM/dd");
                    (flowDocument.FindName("OrderNumberRun") as Run).Text = shipmentToPrint.SalesOrder.SalesOrderNumber;
                    (flowDocument.FindName("CustomerNameRun") as Run).Text = shipmentToPrint.SalesOrder.Customer.CustomerName;
                    (flowDocument.FindName("ShippingAddressRun") as Run).Text = shipmentToPrint.SalesOrder.Customer.ShippingAddress ?? shipmentToPrint.SalesOrder.Customer.BillingAddress;

                    var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                    int counter = 1;
                    var cellBorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2a4d7a"));

                    foreach (var item in shipmentToPrint.ShipmentItems)
                    {
                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(counter.ToString()))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.ProductCode))) { Padding = new Thickness(5), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))) { Padding = new Thickness(5), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString()))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 0, 1) });
                        itemsTableGroup.Rows.Add(row);
                        counter++;
                    }

                    ShowPrintPreview(flowDocument, out tempXpsFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}\n\nتأكد من وجود ملف القالب 'PackingSlipTemplate.xaml'.", "خطأ");
            }
            finally
            {
                CleanupTempFile(tempXpsFile);
            }
        }

        public async Task PrintCustomerStatementAsync(int customerId)
        {
            string tempXpsFile = null;
            try
            {
                using (var db = new DatabaseContext())
                {
                    var customerService = new CustomerService();
                    var customer = await customerService.GetCustomerByIdAsync(customerId);
                    var statementItems = await customerService.GetCustomerStatementAsync(customerId);
                    var companyInfo = db.CompanyInfos.FirstOrDefault();

                    if (customer == null) throw new Exception("لم يتم العثور على العميل.");

                    var resourceUri = new Uri("/UI/Views/CustomerStatementTemplate.xaml", UriKind.Relative);
                    var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                    var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["CustomerStatement"])) as FlowDocument;

                    (flowDocument.FindName("CompanyNameRun") as Run).Text = companyInfo?.CompanyName ?? "اسم الشركة";
                    (flowDocument.FindName("CompanyAddressRun") as Run).Text = companyInfo?.Address ?? "";
                    (flowDocument.FindName("CompanyPhoneRun") as Run).Text = companyInfo?.PhoneNumber ?? "";
                    (flowDocument.FindName("CustomerNameRun") as Run).Text = customer.CustomerName;
                    (flowDocument.FindName("CustomerAddressRun") as Run).Text = customer.BillingAddress ?? "";
                    (flowDocument.FindName("ReportDateRun") as Run).Text = DateTime.Now.ToString("yyyy/MM/dd");

                    var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                    string currencySymbol = AppSettings.DefaultCurrencySymbol;
                    foreach (var item in statementItems)
                    {
                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Date.ToString("yyyy/MM/dd")))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.TransactionType))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.ReferenceNumber))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Debit > 0 ? $"{item.Debit:N2} {currencySymbol}" : "-")) { TextAlignment = TextAlignment.Left }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Credit > 0 ? $"{item.Credit:N2} {currencySymbol}" : "-")) { TextAlignment = TextAlignment.Left }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.Balance:N2} {currencySymbol}")) { TextAlignment = TextAlignment.Left, FontWeight = FontWeights.Bold }));
                        itemsTableGroup.Rows.Add(row);
                    }

                    var totalDebit = statementItems.Sum(i => i.Debit);
                    var totalCredit = statementItems.Sum(i => i.Credit);
                    var finalBalance = statementItems.LastOrDefault()?.Balance ?? 0;
                    (flowDocument.FindName("TotalDebitRun") as Run).Text = $"{totalDebit:N2} {currencySymbol}";
                    (flowDocument.FindName("TotalCreditRun") as Run).Text = $"{totalCredit:N2} {currencySymbol}";
                    (flowDocument.FindName("FinalBalanceRun") as Run).Text = $"{finalBalance:N2} {currencySymbol}";

                    ShowPrintPreview(flowDocument, out tempXpsFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}", "خطأ");
            }
            finally
            {
                CleanupTempFile(tempXpsFile);
            }
        }

        public async Task PrintSalesQuotationAsync(int quotationId)
        {
            string tempXpsFile = null;
            try
            {
                using (var db = new DatabaseContext())
                {
                    var quotation = await db.SalesQuotations
                                            .Include(q => q.Customer)
                                            .Include(q => q.SalesQuotationItems)
                                            .ThenInclude(i => i.Product)
                                            .FirstOrDefaultAsync(q => q.Id == quotationId);

                    if (quotation == null)
                    {
                        MessageBox.Show("لم يتم العثور على عرض السعر المطلوب.", "خطأ");
                        return;
                    }

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    string currencySymbol = AppSettings.DefaultCurrencySymbol;
                    var resourceUri = new Uri("/UI/Views/QuotationPrintTemplate.xaml", UriKind.Relative);
                    var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                    var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["SalesQuotation"])) as FlowDocument;

                    if (companyInfo != null)
                    {
                        (flowDocument.FindName("CompanyNameTextBlock") as Run).Text = companyInfo.CompanyName ?? "اسم الشركة";
                        (flowDocument.FindName("CompanyAddressTextBlock") as Run).Text = companyInfo.Address ?? "";
                        (flowDocument.FindName("CompanyPhoneTextBlock") as Run).Text = companyInfo.PhoneNumber ?? "";
                        (flowDocument.FindName("CompanyTaxNumberTextBlock") as Run).Text = companyInfo.TaxNumber ?? "";
                        if (companyInfo.Logo != null)
                        {
                            var logoImage = new BitmapImage();
                            using (var stream = new MemoryStream(companyInfo.Logo))
                            {
                                logoImage.BeginInit(); logoImage.StreamSource = stream;
                                logoImage.CacheOption = BitmapCacheOption.OnLoad; logoImage.EndInit(); logoImage.Freeze();
                            }
                            (flowDocument.FindName("CompanyLogoImage") as Image).Source = logoImage;
                        }
                    }
                    (flowDocument.FindName("QuotationNumberRun") as Run).Text = quotation.QuotationNumber;
                    (flowDocument.FindName("QuotationDateRun") as Run).Text = quotation.QuotationDate.ToString("yyyy/MM/dd");
                    (flowDocument.FindName("ValidUntilDateRun") as Run).Text = quotation.ValidUntilDate.ToString("yyyy/MM/dd");
                    (flowDocument.FindName("CustomerNameRun") as Run).Text = quotation.Customer.CustomerName;
                    (flowDocument.FindName("TotalAmountRun") as Run).Text = $"{quotation.TotalAmount:N2} {currencySymbol}";
                    (flowDocument.FindName("TotalInWordsRun") as Run).Text = TafqeetHelper.ToWords(quotation.TotalAmount, "جنيه مصري", "قرشاً");

                    var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                    foreach (var item in quotation.SalesQuotationItems)
                    {
                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name)) { Padding = new Thickness(5) }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.UnitPrice:N2} {currencySymbol}")) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5) }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5) }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.Discount:N2} {currencySymbol}")) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5) }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{((item.UnitPrice * item.Quantity) - item.Discount):N2} {currencySymbol}")) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5) }));
                        itemsTableGroup.Rows.Add(row);
                    }
                    ShowPrintPreview(flowDocument, out tempXpsFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}\n\nتأكد من وجود ملف القالب 'QuotationPrintTemplate.xaml' في المسار الصحيح.", "خطأ فادح");
            }
            finally
            {
                CleanupTempFile(tempXpsFile);
            }
        }

        public async Task PrintPurchaseInvoiceAsync(int purchaseId)
        {
            string tempXpsFile = null;
            try
            {
                using (var db = new DatabaseContext())
                {
                    var purchase = await db.Purchases
                        .Include(p => p.Supplier)
                        .Include(p => p.PurchaseItems)
                        .ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(p => p.Id == purchaseId);

                    if (purchase == null) { return; }

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    string currencySymbol = AppSettings.DefaultCurrencySymbol;
                    var resourceUri = new Uri("/UI/Views/PurchaseInvoicePrintTemplate.xaml", UriKind.Relative);
                    var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                    var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["PurchaseInvoice"])) as FlowDocument;

                    if (companyInfo != null)
                    {
                        (flowDocument.FindName("CompanyNameRun") as TextBlock).Text = companyInfo.CompanyName ?? "اسم الشركة";
                        (flowDocument.FindName("CompanyAddressRun") as TextBlock).Text = companyInfo.Address ?? "";
                        (flowDocument.FindName("CompanyPhoneRun") as TextBlock).Text = companyInfo.PhoneNumber ?? "";
                        if (companyInfo.Logo != null)
                        {
                            var logoImage = new BitmapImage();
                            using (var stream = new MemoryStream(companyInfo.Logo))
                            {
                                logoImage.BeginInit(); logoImage.StreamSource = stream;
                                logoImage.CacheOption = BitmapCacheOption.OnLoad; logoImage.EndInit(); logoImage.Freeze();
                            }
                            (flowDocument.FindName("CompanyLogoImage") as Image).Source = logoImage;
                        }
                    }

                    (flowDocument.FindName("SupplierNameRun") as TextBlock).Text = purchase.Supplier.Name;
                    (flowDocument.FindName("SupplierAddressRun") as TextBlock).Text = purchase.Supplier.Address ?? "";
                    (flowDocument.FindName("SupplierTaxNumberRun") as TextBlock).Text = purchase.Supplier.TaxNumber ?? "";
                    (flowDocument.FindName("InvoiceNumberRun") as Run).Text = purchase.InvoiceNumber;
                    (flowDocument.FindName("InvoiceDateRun") as Run).Text = purchase.PurchaseDate.ToString("yyyy/MM/dd");
                    (flowDocument.FindName("DueDateRun") as Run).Text = purchase.DueDate?.ToString("yyyy/MM/dd") ?? "غير محدد";

                    decimal subtotal = purchase.PurchaseItems.Sum(item => item.Quantity * item.UnitPrice);
                    decimal taxAmount = purchase.TotalAmount - subtotal;

                    (flowDocument.FindName("SubtotalRun") as TextBlock).Text = $"{subtotal:N2} {currencySymbol}";
                    (flowDocument.FindName("TaxAmountRun") as TextBlock).Text = $"{taxAmount:N2} {currencySymbol}";
                    (flowDocument.FindName("TotalAmountRun") as TextBlock).Text = $"{purchase.TotalAmount:N2} {currencySymbol}";
                    (flowDocument.FindName("TotalInWordsRun") as TextBlock).Text = TafqeetHelper.ToWords(purchase.TotalAmount, AppSettings.DefaultCurrencyName_AR, AppSettings.DefaultFractionalUnit_AR);

                    var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                    // ======================= بداية الإصلاح الرئيسي =======================
                    // تجميع البنود حسب المنتج لمنع تكرارها بسبب الضرائب أو أي إضافات أخرى
                    var groupedItems = purchase.PurchaseItems
                        .GroupBy(item => item.ProductId)
                        .Select(g => new
                        {
                            Product = g.First().Product,
                            Quantity = g.Sum(item => item.Quantity),
                            UnitPrice = g.First().UnitPrice // افتراض أن السعر موحد للمنتج الواحد
                        });

                    foreach (var item in groupedItems)
                    {
                        decimal itemTotal = item.Quantity * item.UnitPrice;
                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))) { Padding = new Thickness(8) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString("N2")))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(8) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.UnitPrice:N2}"))) { TextAlignment = TextAlignment.Right, Padding = new Thickness(8) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{itemTotal:N2}"))) { TextAlignment = TextAlignment.Right, Padding = new Thickness(8) });
                        itemsTableGroup.Rows.Add(row);
                    }
                    // ======================== نهاية الإصلاح الرئيسي ========================

                    ShowPrintPreview(flowDocument, out tempXpsFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الطباعة: {ex.ToString()}", "خطأ فادح");
            }
            finally
            {
                CleanupTempFile(tempXpsFile);
            }
        }

        public async Task PrintSalesInvoiceAsync(int saleId)
        {
            string tempXpsFile = null;
            try
            {
                using (var db = new DatabaseContext())
                {
                    var saleToPrint = await db.Sales
                        .Include(s => s.Customer)
                        .Include(s => s.SalesOrder)
                        .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                        .Include(s => s.SalesReturns)
                        .FirstOrDefaultAsync(s => s.Id == saleId);

                    if (saleToPrint == null) { return; }

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    var resourceUri = new Uri("/UI/Views/SalesInvoiceTemplate.xaml", UriKind.Relative);
                    var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                    var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["SalesInvoice"])) as FlowDocument;

                    (flowDocument.FindName("CompanyNameTextBlock") as Run).Text = companyInfo?.CompanyName ?? "اسم الشركة";
                    (flowDocument.FindName("CompanyAddressTextBlock") as Run).Text = companyInfo?.Address;
                    (flowDocument.FindName("CompanyPhoneTextBlock") as Run).Text = companyInfo?.PhoneNumber;
                    (flowDocument.FindName("CompanyTaxNumberTextBlock") as Run).Text = companyInfo?.TaxNumber;
                    if (companyInfo?.Logo != null)
                    {
                        var logoImage = new BitmapImage();
                        using (var stream = new MemoryStream(companyInfo.Logo))
                        {
                            logoImage.BeginInit(); logoImage.StreamSource = stream;
                            logoImage.CacheOption = BitmapCacheOption.OnLoad; logoImage.EndInit(); logoImage.Freeze();
                        }
                        (flowDocument.FindName("CompanyLogoImage") as Image).Source = logoImage;
                    }

                    (flowDocument.FindName("InvoiceNumberRun") as Run).Text = saleToPrint.InvoiceNumber;
                    (flowDocument.FindName("SaleDateRun") as Run).Text = saleToPrint.SaleDate.ToString("yyyy/MM/dd");
                    (flowDocument.FindName("SalesOrderNumberRun") as Run).Text = saleToPrint.SalesOrder?.SalesOrderNumber ?? string.Empty;
                    (flowDocument.FindName("CustomerNameRun") as Run).Text = saleToPrint.Customer?.CustomerName ?? string.Empty;
                    (flowDocument.FindName("CustomerTaxNumberRun") as Run).Text = saleToPrint.Customer?.TaxNumber ?? string.Empty;
                    (flowDocument.FindName("CustomerAddressRun") as Run).Text = saleToPrint.Customer?.BillingAddress ?? string.Empty;

                    var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                    int rowIndex = 1;
                    var cellBorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2a4d7a"));
                    foreach (var item in saleToPrint.SaleItems)
                    {
                        var row = new TableRow();
                        decimal discount = 0;
                        decimal total = (item.Quantity * item.UnitPrice) - discount;
                        row.Cells.Add(new TableCell(new Paragraph(new Run(rowIndex.ToString()))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(4), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))) { Padding = new Thickness(4), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString()))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(4), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.UnitPrice:N2}"))) { TextAlignment = TextAlignment.Right, Padding = new Thickness(4), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{discount:N2}"))) { TextAlignment = TextAlignment.Right, Padding = new Thickness(4), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{total:N2}"))) { TextAlignment = TextAlignment.Right, Padding = new Thickness(4), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 0, 1) });
                        itemsTableGroup.Rows.Add(row);
                        rowIndex++;
                    }

                    (flowDocument.FindName("SubTotalRun") as Run).Text = $"{saleToPrint.Subtotal:N2} {AppSettings.DefaultCurrencySymbol}";
                    (flowDocument.FindName("VatAmountRun") as Run).Text = $"{saleToPrint.TaxAmount:N2} {AppSettings.DefaultCurrencySymbol}";
                    (flowDocument.FindName("TotalAmountRun") as Run).Text = $"{saleToPrint.TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
                    (flowDocument.FindName("AmountPaidRun") as Run).Text = $"{saleToPrint.AmountPaid:N2} {AppSettings.DefaultCurrencySymbol}";

                    string currencyName = !string.IsNullOrWhiteSpace(AppSettings.DefaultCurrencyName_AR) ? AppSettings.DefaultCurrencyName_AR : "ريال سعودي";
                    string fractionalUnit = !string.IsNullOrWhiteSpace(AppSettings.DefaultFractionalUnit_AR) ? AppSettings.DefaultFractionalUnit_AR : "هللة";

                    (flowDocument.FindName("TotalInWordsRun") as Run).Text = TafqeetHelper.ToWords(
                        saleToPrint.TotalAmount, currencyName, fractionalUnit);

                    decimal totalReturnedValue = saleToPrint.SalesReturns?.Sum(r => r.TotalReturnValue) ?? 0;
                    decimal finalBalance = saleToPrint.TotalAmount - saleToPrint.AmountPaid - totalReturnedValue;

                    var amountDueRow = flowDocument.FindName("AmountDueRow") as TableRow;
                    var amountDueInWordsParagraph = flowDocument.FindName("AmountDueInWordsParagraph") as Paragraph;

                    if (Math.Abs(finalBalance) > 0.009m)
                    {
                        var amountDueLabelCell = amountDueRow?.Cells.FirstOrDefault();
                        if (finalBalance > 0)
                        {
                            if (amountDueLabelCell != null) ((amountDueLabelCell.Blocks.FirstBlock as Paragraph).Inlines.FirstInline as Run).Text = "المتبقي:";
                            (flowDocument.FindName("AmountDueRun") as Run).Text = $"{finalBalance:N2} {AppSettings.DefaultCurrencySymbol}";
                            (flowDocument.FindName("AmountDueInWordsRun") as Run).Text = TafqeetHelper.ToWords(finalBalance, currencyName, fractionalUnit);
                        }
                        else
                        {
                            if (amountDueLabelCell != null) ((amountDueLabelCell.Blocks.FirstBlock as Paragraph).Inlines.FirstInline as Run).Text = "رصيد دائن للعميل:";
                            (flowDocument.FindName("AmountDueRun") as Run).Text = $"{Math.Abs(finalBalance):N2} {AppSettings.DefaultCurrencySymbol}";
                            if (amountDueInWordsParagraph != null && amountDueInWordsParagraph.Parent is TableCell parentCell)
                                parentCell.Blocks.Remove(amountDueInWordsParagraph);
                        }
                    }
                    else
                    {
                        if (amountDueRow != null && amountDueRow.Parent is TableRowGroup parentRowGroup)
                            parentRowGroup.Rows.Remove(amountDueRow);

                        if (amountDueInWordsParagraph != null && amountDueInWordsParagraph.Parent is TableCell parentCell)
                            parentCell.Blocks.Remove(amountDueInWordsParagraph);
                    }

                    ShowPrintPreview(flowDocument, out tempXpsFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الطباعة: {ex.ToString()}", "خطأ فادح");
            }
            finally
            {
                CleanupTempFile(tempXpsFile);
            }
        }

        public async Task PrintPurchaseOrderAsync(int purchaseOrderId)
        {
            string tempXpsFile = null;
            try
            {
                using (var db = new DatabaseContext())
                {
                    var po = await db.PurchaseOrders
                                .Include(p => p.Supplier)
                                .Include(p => p.PurchaseOrderItems)
                                .ThenInclude(i => i.Product)
                                .FirstOrDefaultAsync(p => p.Id == purchaseOrderId);

                    if (po == null)
                    {
                        MessageBox.Show("لم يتم العثور على أمر الشراء.", "خطأ");
                        return;
                    }

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    var resourceUri = new Uri("/UI/Views/PurchaseOrderPrintTemplate.xaml", UriKind.Relative);
                    var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                    var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["PurchaseOrder"])) as FlowDocument;

                    if (companyInfo != null)
                    {
                        (flowDocument.FindName("CompanyNameTextBlock") as Run).Text = companyInfo.CompanyName ?? "اسم الشركة";
                        (flowDocument.FindName("CompanyAddressTextBlock") as Run).Text = companyInfo.Address;
                        (flowDocument.FindName("CompanyPhoneTextBlock") as Run).Text = companyInfo.PhoneNumber;
                        if (companyInfo.Logo != null)
                        {
                            var logoImage = new BitmapImage();
                            using (var stream = new MemoryStream(companyInfo.Logo))
                            {
                                logoImage.BeginInit(); logoImage.StreamSource = stream;
                                logoImage.CacheOption = BitmapCacheOption.OnLoad; logoImage.EndInit(); logoImage.Freeze();
                            }
                            (flowDocument.FindName("CompanyLogoImage") as Image).Source = logoImage;
                        }
                    }

                    (flowDocument.FindName("SupplierNameRun") as Run).Text = po.Supplier.Name;
                    (flowDocument.FindName("SupplierAddressRun") as Run).Text = po.Supplier.Address ?? "";
                    (flowDocument.FindName("SupplierTaxNumberRun") as Run).Text = po.Supplier.TaxNumber ?? "";
                    (flowDocument.FindName("PONumberRun") as Run).Text = po.PurchaseOrderNumber;
                    (flowDocument.FindName("OrderDateRun") as Run).Text = po.OrderDate.ToString("yyyy/MM/dd");
                    (flowDocument.FindName("DeliveryDateRun") as Run).Text = po.ExpectedDeliveryDate?.ToString("yyyy/MM/dd") ?? "غير محدد";

                    var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                    int rowIndex = 1;
                    foreach (var item in po.PurchaseOrderItems)
                    {
                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(rowIndex.ToString())) { TextAlignment = TextAlignment.Center }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { TextAlignment = TextAlignment.Center }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.UnitPrice:N2}"))) { TextAlignment = TextAlignment.Center });
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{(item.Quantity * item.UnitPrice):N2}"))) { TextAlignment = TextAlignment.Center });
                        itemsTableGroup.Rows.Add(row);
                        rowIndex++;
                    }

                    (flowDocument.FindName("TotalAmountRun") as Run).Text = $"{po.TotalAmount:N2}";
                    (flowDocument.FindName("TotalInWordsRun") as Run).Text = TafqeetHelper.ToWords(po.TotalAmount, AppSettings.DefaultCurrencyName_AR, AppSettings.DefaultFractionalUnit_AR);

                    ShowPrintPreview(flowDocument, out tempXpsFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}", "خطأ");
            }
            finally
            {
                CleanupTempFile(tempXpsFile);
            }
        }

        // --- بداية الإضافة: تطبيق دالة طباعة سند الاستلام ---
        public async Task PrintGoodsReceiptNoteAsync(int grnId)
        {
            string tempXpsFile = null;
            try
            {
                using (var db = new DatabaseContext())
                {
                    var grn = await db.GoodsReceiptNotes
                                .Include(g => g.PurchaseOrder.Supplier)
                                .Include(g => g.GoodsReceiptNoteItems)
                                .ThenInclude(i => i.Product)
                                .FirstOrDefaultAsync(g => g.Id == grnId);

                    if (grn == null)
                    {
                        MessageBox.Show("لم يتم العثور على مذكرة الاستلام.", "خطأ");
                        return;
                    }

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    var resourceUri = new Uri("/UI/Views/GrnPrintTemplate.xaml", UriKind.Relative);
                    var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                    var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["GoodsReceiptNote"])) as FlowDocument;

                    // تعبئة بيانات الشركة
                    if (companyInfo != null)
                    {
                        (flowDocument.FindName("CompanyNameRun") as Run).Text = companyInfo.CompanyName ?? "اسم الشركة";
                        (flowDocument.FindName("CompanyAddressRun") as Run).Text = companyInfo.Address;
                        (flowDocument.FindName("CompanyPhoneRun") as Run).Text = companyInfo.PhoneNumber;
                        (flowDocument.FindName("CompanyTaxNumberTextBlock") as Run).Text = $"الرقم الضريبي: {companyInfo.TaxNumber ?? "-"}";
                        if (companyInfo.Logo != null)
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
                    }

                    // تعبئة بيانات السند
                    (flowDocument.FindName("GrnNumberRun") as Run).Text = grn.GRNNumber;
                    (flowDocument.FindName("ReceiptDateRun") as Run).Text = grn.ReceiptDate.ToString("yyyy/MM/dd");
                    (flowDocument.FindName("SupplierNameRun") as Run).Text = grn.PurchaseOrder.Supplier.Name;
                    (flowDocument.FindName("PoNumberRun") as Run).Text = grn.PurchaseOrder.PurchaseOrderNumber;

                    // تعبئة البنود
                    var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                    int counter = 1;
                    foreach (var item in grn.GoodsReceiptNoteItems)
                    {
                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(counter.ToString()))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.ProductCode))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.QuantityReceived.ToString())) { TextAlignment = TextAlignment.Center }));
                        itemsTableGroup.Rows.Add(row);
                        counter++;
                    }

                    // عرض نافذة معاينة الطباعة
                    ShowPrintPreview(flowDocument, out tempXpsFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}\n\n{ex.InnerException?.Message}", "خطأ");
            }
            finally
            {
                CleanupTempFile(tempXpsFile);
            }
        }
        // --- نهاية الإضافة ---

        // ======================= بداية الإضافة =======================
        public async Task PrintBomAsync(int bomId)
        {
            string tempXpsFile = null;
            try
            {
                using (var db = new DatabaseContext())
                {
                    var bom = await db.BillOfMaterials
                        .Include(b => b.FinishedGood)
                        .Include(b => b.BillOfMaterialsItems)
                        .ThenInclude(i => i.RawMaterial)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(b => b.Id == bomId);

                    if (bom == null)
                    {
                        MessageBox.Show("لم يتم العثور على قائمة المكونات.", "خطأ");
                        return;
                    }

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    var resourceUri = new Uri("/UI/Views/BomPrintTemplate.xaml", UriKind.Relative);
                    var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                    var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["BillOfMaterials"])) as FlowDocument;

                    // ======================= بداية التحديث =======================
                    // تعبئة بيانات الشركة (الترويسة)
                    if (companyInfo != null)
                    {
                        (flowDocument.FindName("CompanyNameTextBlock") as TextBlock).Text = companyInfo.CompanyName ?? "اسم الشركة";
                        (flowDocument.FindName("CompanyAddressTextBlock") as TextBlock).Text = companyInfo.Address ?? "";
                        (flowDocument.FindName("CompanyPhoneTextBlock") as TextBlock).Text = companyInfo.PhoneNumber ?? "";
                        if (companyInfo.Logo != null)
                        {
                            var logoImage = new BitmapImage();
                            using (var stream = new MemoryStream(companyInfo.Logo))
                            {
                                logoImage.BeginInit(); logoImage.StreamSource = stream;
                                logoImage.CacheOption = BitmapCacheOption.OnLoad; logoImage.EndInit(); logoImage.Freeze();
                            }
                            (flowDocument.FindName("CompanyLogoImage") as Image).Source = logoImage;
                        }
                    }

                    // ملء بيانات التقرير
                    (flowDocument.FindName("ProductNameRun") as Run).Text = bom.FinishedGood.Name;
                    (flowDocument.FindName("ProductCodeRun") as Run).Text = bom.FinishedGood.ProductCode;
                    (flowDocument.FindName("DescriptionRun") as Run).Text = bom.Description;
                    (flowDocument.FindName("PrintDateRun") as Run).Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm tt");

                    var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                    var cellBorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#AAAAAA"));

                    foreach (var item in bom.BillOfMaterialsItems)
                    {
                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.RawMaterial.ProductCode))) { Padding = new Thickness(8), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.RawMaterial.Name))) { Padding = new Thickness(8), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 1, 1) });
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString("0.####")))) { TextAlignment = TextAlignment.Center, Padding = new Thickness(8), BorderBrush = cellBorderBrush, BorderThickness = new Thickness(0, 0, 0, 1) });
                        itemsTableGroup.Rows.Add(row);
                    }
                    // ======================== نهاية التحديث ========================

                    ShowPrintPreview(flowDocument, out tempXpsFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}\n\nتأكد من وجود ملف القالب 'BomPrintTemplate.xaml'.", "خطأ");
            }
            finally
            {
                CleanupTempFile(tempXpsFile);
            }
        }
        // ======================== نهاية الإضافة ========================


        private void ShowPrintPreview(FlowDocument document, out string tempFilePath)
        {
            tempFilePath = Path.GetTempFileName();
            using (var xpsDoc = new XpsDocument(tempFilePath, FileAccess.ReadWrite))
            {
                var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
                var xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                xpsWriter.Write(paginator);
                var previewWindow = new PrintPreviewWindow(xpsDoc.GetFixedDocumentSequence(), tempFilePath);
                previewWindow.ShowDialog();
            }
        }

        private void CleanupTempFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try { File.Delete(filePath); } catch { /* Ignore */ }
            }
        }

        private double CalculateScale(PrintDialog printDialog, FrameworkElement visual)
        {
            double scaleX = printDialog.PrintableAreaWidth / visual.ActualWidth;
            double scaleY = printDialog.PrintableAreaHeight / visual.ActualHeight;
            return Math.Min(scaleX, scaleY);
        }
    }
}