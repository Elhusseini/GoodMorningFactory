// UI/Views/PurchaseOrdersView.xaml.cs
// *** الكود الكامل لواجهة أوامر الشراء مع إضافة منطق زر إنشاء الفاتورة ***
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
                    PurchaseOrdersDataGrid.ItemsSource = query.OrderByDescending(po => po.OrderDate).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
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

        // --- بداية الإصلاح: تعديل منطق زر التعديل ---
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseOrder order)
            {
                var editWindow = new AddEditPurchaseOrderWindow(order.Id);
                if (editWindow.ShowDialog() == true) { LoadPurchaseOrders(); }
            }
        }
        // --- نهاية الإصلاح ---

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseOrder orderToCancel)
            {
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
            if ((sender as Button)?.DataContext is PurchaseOrder orderToPrint)
            {
                XpsDocument xpsDoc = null;
                Uri packUri = null;

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var po = db.PurchaseOrders
                                  .Include(p => p.Supplier)
                                  .Include(p => p.PurchaseOrderItems)
                                  .ThenInclude(i => i.Product)
                                  .FirstOrDefault(p => p.Id == orderToPrint.Id);

                        if (po == null) return;

                        var resourceUri = new Uri("/UI/Views/PurchaseOrderPrintTemplate.xaml", UriKind.Relative);
                        var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                        var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["PurchaseOrder"])) as FlowDocument;

                        (flowDocument.FindName("PONumberRun") as Run).Text = po.PurchaseOrderNumber;
                        (flowDocument.FindName("OrderDateRun") as Run).Text = po.OrderDate.ToString("d");
                        (flowDocument.FindName("SupplierNameRun") as Run).Text = po.Supplier.Name;
                        (flowDocument.FindName("DeliveryDateRun") as Run).Text = po.ExpectedDeliveryDate?.ToString("d");
                        (flowDocument.FindName("TotalAmountRun") as Run).Text = po.TotalAmount.ToString("C");

                        var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                        foreach (var item in po.PurchaseOrderItems)
                        {
                            var row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString()))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.UnitPrice.ToString("C")))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run((item.Quantity * item.UnitPrice).ToString("C")))));
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
                catch (Exception ex) { MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}", "خطأ"); }
                finally
                {
                    xpsDoc?.Close();
                    if (packUri != null && PackageStore.GetPackage(packUri) != null) { PackageStore.RemovePackage(packUri); }
                }
            }
        }

        private void ReceiveGoodsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseOrder order)
            {
                if (order.Status == PurchaseOrderStatus.FullyReceived || order.Status == PurchaseOrderStatus.Cancelled)
                {
                    MessageBox.Show("لا يمكن تسجيل استلام لهذا الأمر.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var receiveWindow = new AddGoodsReceiptWindow(order.Id);
                if (receiveWindow.ShowDialog() == true)
                {
                    LoadPurchaseOrders();
                }
            }
        }

        // --- بداية التحديث: إضافة دالة إنشاء فاتورة من أمر شراء ---
        private void CreateInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseOrder order)
            {
                // فتح نافذة تسجيل الفاتورة مع تمرير هوية أمر الشراء
                var invoiceWindow = new AddEditPurchaseInvoiceWindow(purchaseOrderId: order.Id);
                if (invoiceWindow.ShowDialog() == true)
                {
                    // تحديث القائمة بعد إنشاء الفاتورة لتغيير حالة أمر الشراء
                    LoadPurchaseOrders();
                }
            }
        }
        // --- نهاية التحديث ---
    }
}