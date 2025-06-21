// UI/Views/BillOfMaterialsView.xaml.cs
// *** الكود الكامل للكود الخلفي لواجهة قوائم المكونات مع إصلاح استدعاء نافذة النسخ ***
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
    public partial class BillOfMaterialsView : UserControl
    {
        public BillOfMaterialsView()
        {
            InitializeComponent();
            LoadBoms();
        }

        private void LoadBoms()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    BomDataGrid.ItemsSource = db.BillOfMaterials.Include(b => b.FinishedGood).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل قوائم المكونات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddBomButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditBillOfMaterialsWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadBoms();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is BillOfMaterials bom)
            {
                var editWindow = new AddEditBillOfMaterialsWindow(bomId: bom.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadBoms();
                }
            }
        }

        // --- بداية الإصلاح: تعديل طريقة استدعاء نافذة النسخ ---
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is BillOfMaterials bomToCopy)
            {
                // فتح نافذة الإنشاء مع تمرير هوية القائمة المراد نسخها
                var copyWindow = new AddEditBillOfMaterialsWindow(sourceBomIdToCopy: bomToCopy.Id);
                if (copyWindow.ShowDialog() == true)
                {
                    LoadBoms();
                }
            }
        }
        // --- نهاية الإصلاح ---

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is BillOfMaterials bomToPrint)
            {
                XpsDocument xpsDoc = null;
                Uri packUri = null;

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var bom = db.BillOfMaterials
                                  .Include(b => b.FinishedGood)
                                  .Include(b => b.BillOfMaterialsItems)
                                  .ThenInclude(i => i.RawMaterial)
                                  .FirstOrDefault(b => b.Id == bomToPrint.Id);

                        if (bom == null) return;

                        var resourceUri = new Uri("/UI/Views/BomPrintTemplate.xaml", UriKind.Relative);
                        var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                        var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["BillOfMaterials"])) as FlowDocument;

                        (flowDocument.FindName("ProductNameRun") as Run).Text = bom.FinishedGood.Name;
                        (flowDocument.FindName("ProductCodeRun") as Run).Text = bom.FinishedGood.ProductCode;
                        (flowDocument.FindName("DescriptionRun") as Run).Text = bom.Description;

                        var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                        foreach (var item in bom.BillOfMaterialsItems)
                        {
                            var row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.RawMaterial.ProductCode))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.RawMaterial.Name))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString("N4")))));
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
                    if (packUri != null) { PackageStore.RemovePackage(packUri); }
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is BillOfMaterials bomToDelete)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف قائمة المكونات للمنتج '{bomToDelete.FinishedGood.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            var items = db.BillOfMaterialsItems.Where(i => i.BillOfMaterialsId == bomToDelete.Id);
                            db.BillOfMaterialsItems.RemoveRange(items);
                            db.BillOfMaterials.Attach(bomToDelete);
                            db.BillOfMaterials.Remove(bomToDelete);
                            db.SaveChanges();
                            LoadBoms();
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