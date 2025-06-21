// UI/Views/PurchaseOrdersView.xaml.cs
// *** تحديث: تم إصلاح خطأ "InvalidOperationException" عند إنشاء فاتورة ***
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
    public partial class PurchaseOrdersView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public PurchaseOrdersView()
        {
            InitializeComponent();

            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(PurchaseOrderStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;

            LoadPurchaseOrders();
        }

        private void LoadPurchaseOrders()
        {
            try
            {
                string searchText = SearchTextBox.Text.ToLower();

                using (var db = new DatabaseContext())
                {
                    var query = db.PurchaseOrders.Include(po => po.Supplier).AsQueryable();

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(po => po.PurchaseOrderNumber.ToLower().Contains(searchText) || po.Supplier.Name.ToLower().Contains(searchText));
                    }
                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is PurchaseOrderStatus status)
                    {
                        query = query.Where(po => po.Status == status);
                    }

                    _totalItems = query.Count();

                    var ordersForPage = query.OrderByDescending(po => po.OrderDate)
                                             .Skip((_currentPage - 1) * _pageSize)
                                             .Take(_pageSize)
                                             .ToList();

                    var viewModels = new List<PurchaseOrderViewModel>();
                    foreach (var order in ordersForPage)
                    {
                        viewModels.Add(new PurchaseOrderViewModel { Order = order });
                    }
                    PurchaseOrdersDataGrid.ItemsSource = viewModels;

                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل أوامر الشراء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadPurchaseOrders(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage < (int)Math.Ceiling((double)_totalItems / _pageSize)) { _currentPage++; LoadPurchaseOrders(); } }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadPurchaseOrders(); } }
        private void Filter_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { _currentPage = 1; LoadPurchaseOrders(); } }

        private void AddPurchaseOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditPurchaseOrderWindow();
            if (addWindow.ShowDialog() == true) { LoadPurchaseOrders(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseOrderViewModel vm)
            {
                var editWindow = new AddEditPurchaseOrderWindow(vm.Order.Id);
                if (editWindow.ShowDialog() == true) { LoadPurchaseOrders(); }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseOrderViewModel vm)
            {
                var orderToCancel = vm.Order;
                var result = MessageBox.Show($"هل أنت متأكد من إلغاء أمر الشراء رقم '{orderToCancel.PurchaseOrderNumber}'؟", "تأكيد الإلغاء", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new DatabaseContext())
                    {
                        var order = db.PurchaseOrders.Find(orderToCancel.Id);
                        order.Status = PurchaseOrderStatus.Cancelled;
                        db.SaveChanges();
                        LoadPurchaseOrders();
                    }
                }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseOrderViewModel vm)
            {
                var orderToPrint = vm.Order;
                string tempXpsFile = null;
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var po = db.PurchaseOrders
                                  .Include(p => p.Supplier)
                                  .Include(p => p.PurchaseOrderItems)
                                  .ThenInclude(i => i.Product)
                                  .FirstOrDefault(p => p.Id == orderToPrint.Id);

                        if (po == null)
                        {
                            MessageBox.Show("لم يتم العثور على أمر الشراء.", "خطأ");
                            return;
                        }

                        var companyInfo = db.CompanyInfos.FirstOrDefault();
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
                                    logoImage.BeginInit();
                                    logoImage.StreamSource = stream;
                                    logoImage.CacheOption = BitmapCacheOption.OnLoad;
                                    logoImage.EndInit();
                                    logoImage.Freeze();
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
                            row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.UnitPrice:N2} {AppSettings.DefaultCurrencySymbol}")) { TextAlignment = TextAlignment.Center }));
                            row.Cells.Add(new TableCell(new Paragraph(new Run($"{(item.Quantity * item.UnitPrice):N2} {AppSettings.DefaultCurrencySymbol}")) { TextAlignment = TextAlignment.Center }));
                            itemsTableGroup.Rows.Add(row);
                            rowIndex++;
                        }

                        (flowDocument.FindName("TotalAmountRun") as Run).Text = $"{po.TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
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
                        (flowDocument.FindName("TotalInWordsRun") as Run).Text = Core.Helpers.TafqeetHelper.ToWords(po.TotalAmount, currencyNameAr, fractionalNameAr);

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
                    MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}\n\nتأكد من وجود ملف القالب 'PurchaseOrderPrintTemplate.xaml'.", "خطأ");
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

        private void ReceiveGoodsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseOrderViewModel vm)
            {
                var receiveWindow = new AddGoodsReceiptWindow(vm.Order.Id);
                if (receiveWindow.ShowDialog() == true)
                {
                    LoadPurchaseOrders();
                }
            }
        }

        private void CreateInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseOrderViewModel vm)
            {
                // --- بداية الإصلاح ---
                // 1. التحقق من وجود بضاعة غير مفوترة قبل فتح النافذة
                using (var db = new DatabaseContext())
                {
                    bool hasUninvoicedItems = db.GoodsReceiptNotes
                        .Any(grn => grn.PurchaseOrderId == vm.Order.Id && !grn.IsInvoiced);

                    if (!hasUninvoicedItems)
                    {
                        MessageBox.Show("لا توجد بضاعة مستلمة وغير مفوترة لإنشاء فاتورة لها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                        return; // الخروج من الدالة وعدم فتح النافذة
                    }
                }

                // 2. إذا وجد، يتم فتح النافذة
                var invoiceWindow = new AddEditPurchaseInvoiceWindow(purchaseOrderId: vm.Order.Id);
                if (invoiceWindow.ShowDialog() == true)
                {
                    LoadPurchaseOrders();
                }
                // --- نهاية الإصلاح ---
            }
        }
    }
}
