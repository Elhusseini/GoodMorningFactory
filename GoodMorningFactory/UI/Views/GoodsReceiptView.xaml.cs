// UI/Views/GoodsReceiptView.xaml.cs
// *** تحديث شامل: تم تغيير منطق الطباعة ليعرض نافذة معاينة قبل الطباعة ***
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
using System.Windows.Media.Imaging;
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

        // --- بداية التحديث: دالة الطباعة الجديدة مع المعاينة ---
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GoodsReceiptNote grnToPrint)
            {
                string tempXpsFile = null; // للاحتفاظ بمسار الملف المؤقت

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        // 1. جلب جميع البيانات المطلوبة دفعة واحدة
                        var grn = db.GoodsReceiptNotes
                                  .Include(g => g.PurchaseOrder.Supplier)
                                  .Include(g => g.GoodsReceiptNoteItems)
                                  .ThenInclude(i => i.Product)
                                  .FirstOrDefault(g => g.Id == grnToPrint.Id);

                        var companyInfo = db.CompanyInfos.FirstOrDefault();

                        if (grn == null)
                        {
                            MessageBox.Show("لم يتم العثور على مذكرة الاستلام.", "خطأ");
                            return;
                        }

                        // 2. تحميل قالب XAML
                        var resourceUri = new Uri("/UI/Views/GrnPrintTemplate.xaml", UriKind.Relative);
                        var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                        var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["GoodsReceiptNote"])) as FlowDocument;

                        // 3. تعبئة القالب بالبيانات
                        // معلومات الشركة
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

                        // معلومات مذكرة الاستلام
                        (flowDocument.FindName("GrnNumberRun") as Run).Text = grn.GRNNumber;
                        (flowDocument.FindName("ReceiptDateRun") as Run).Text = grn.ReceiptDate.ToString("yyyy/MM/dd");
                        (flowDocument.FindName("SupplierNameRun") as Run).Text = grn.PurchaseOrder.Supplier.Name;
                        (flowDocument.FindName("PoNumberRun") as Run).Text = grn.PurchaseOrder.PurchaseOrderNumber;

                        // جدول البنود
                        var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                        int counter = 1;
                        foreach (var item in grn.GoodsReceiptNoteItems)
                        {
                            var row = new TableRow();
                            // رقم تسلسلي
                            row.Cells.Add(new TableCell(new Paragraph(new Run(counter.ToString())) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1) }));
                            // كود الصنف
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.ProductCode)) { Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1) }));
                            // وصف الصنف
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name)) { Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1) }));
                            // الكمية المستلمة
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.QuantityReceived.ToString())) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1) }));
                            itemsTableGroup.Rows.Add(row);
                            counter++;
                        }

                        // 4. إنشاء مستند XPS للمعاينة
                        var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                        tempXpsFile = Path.GetTempFileName();

                        using (var xpsDoc = new XpsDocument(tempXpsFile, FileAccess.ReadWrite))
                        {
                            var xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                            xpsWriter.Write(paginator);
                        }

                        // 5. عرض نافذة المعاينة
                        using (var xpsDoc = new XpsDocument(tempXpsFile, FileAccess.Read))
                        {
                            var fixedDocSeq = xpsDoc.GetFixedDocumentSequence();
                            // تمرير مسار الملف المؤقت إلى نافذة المعاينة ليتم حذفه بعد الإغلاق
                            var previewWindow = new PrintPreviewWindow(fixedDocSeq, tempXpsFile);
                            previewWindow.ShowDialog();
                        }

                        // لم يعد هناك حاجة لحذف الملف هنا، حيث تتولى نافذة المعاينة ذلك
                        tempXpsFile = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}\n\n{ex.InnerException?.Message}", "خطأ");
                }
                finally
                {
                    // تنظيف الملف المؤقت في حال حدوث خطأ قبل عرض نافذة المعاينة
                    if (tempXpsFile != null && File.Exists(tempXpsFile))
                    {
                        try { File.Delete(tempXpsFile); } catch { }
                    }
                }
            }
        }
        // --- نهاية التحديث ---
    }
}
