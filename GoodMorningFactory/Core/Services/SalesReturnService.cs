// GoodMorningFactory/Core/Services/SalesReturnService.cs

// --- ملاحظة: هذا الكلاس يحتوي على كل المنطق البرمجي الفعلي للتعامل مع قاعدة البيانات لمرتجعات المبيعات. ---
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using GoodMorningFactory.UI.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;

namespace GoodMorningFactory.Core.Services
{
    public class SalesReturnService : ISalesReturnService
    {
        public async Task<PagedResult<SalesReturn>> GetPagedSalesReturnsAsync(int page, int pageSize, string searchText, DateTime? fromDate, DateTime? toDate)
        {
            // هذه الدالة مسؤولة عن جلب قائمة مرتجعات المبيعات من قاعدة البيانات
            using (var db = new DatabaseContext())
            {
                var query = db.SalesReturns
                    .Include(sr => sr.Sale.Customer) // تضمين العميل لعرض اسمه
                    .AsQueryable();

                // تطبيق فلتر البحث النصي
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string text = searchText.ToLower();
                    query = query.Where(sr => sr.ReturnNumber.ToLower().Contains(text) ||
                                              sr.Sale.InvoiceNumber.ToLower().Contains(text) ||
                                              sr.Sale.Customer.CustomerName.ToLower().Contains(text));
                }
                // تطبيق فلتر التاريخ
                if (fromDate.HasValue) { query = query.Where(sr => sr.ReturnDate >= fromDate.Value.Date); }
                if (toDate.HasValue) { query = query.Where(sr => sr.ReturnDate < toDate.Value.Date.AddDays(1)); }

                int totalItems = await query.CountAsync();
                var items = await query
                    .OrderByDescending(sr => sr.ReturnDate) // ترتيب النتائج بالأحدث أولاً
                    .Skip((page - 1) * pageSize) // تخطي الصفحات السابقة
                    .Take(pageSize) // أخذ عدد معين من السجلات لهذه الصفحة
                    .ToListAsync();

                return new PagedResult<SalesReturn> { Items = items, TotalItems = totalItems };
            }
        }

        public async Task<Sale> GetDataForReturnCreationAsync(int saleId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Sales
                    .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                    .AsNoTracking() // استخدام AsNoTracking لتحسين الأداء لأننا لن نعدل هذه البيانات
                    .FirstOrDefaultAsync(s => s.Id == saleId);
            }
        }

        public async Task<Dictionary<int, int>> GetPreviouslyReturnedQuantitiesAsync(int saleId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.SalesReturnItems
                    .Where(sri => sri.SalesReturn.SaleId == saleId)
                    .GroupBy(sri => sri.ProductId)
                    .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Quantity);
            }
        }

        public async Task CreateSalesReturnAsync(int saleId, IEnumerable<SalesReturnItemViewModel> itemsToReturn)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                // التحقق من وجود الحسابات الافتراضية قبل البدء
                if (companyInfo?.DefaultSalesReturnsAccountId == null || companyInfo?.DefaultAccountsReceivableAccountId == null)
                    throw new Exception("يرجى تحديد حساب 'مردودات المبيعات' و 'الذمم المدينة' الافتراضي في الإعدادات.");

                var defaultLocation = await db.StorageLocations.FirstOrDefaultAsync();
                if (defaultLocation == null)
                    throw new Exception("لا يوجد موقع تخزين افتراضي معرف لاستقبال المرتجعات.");

                var salesReturn = new SalesReturn { ReturnNumber = $"SRTN-{DateTime.Now:yyyyMMddHHmmss}", ReturnDate = DateTime.Today, SaleId = saleId };
                decimal totalReturnValue = 0;

                foreach (var itemVM in itemsToReturn)
                {
                    if (itemVM.QuantityToReturn <= 0) continue;

                    salesReturn.SalesReturnItems.Add(new SalesReturnItem { ProductId = itemVM.ProductId, Quantity = itemVM.QuantityToReturn });
                    totalReturnValue += itemVM.QuantityToReturn * itemVM.UnitPrice;

                    // تحديث رصيد المخزون (زيادة)
                    var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == itemVM.ProductId && i.StorageLocationId == defaultLocation.Id);
                    if (inventory != null) { inventory.Quantity += itemVM.QuantityToReturn; }
                    else { db.Inventories.Add(new Inventory { ProductId = itemVM.ProductId, StorageLocationId = defaultLocation.Id, Quantity = itemVM.QuantityToReturn }); }

                    // تسجيل حركة المخزون
                    db.StockMovements.Add(new StockMovement { ProductId = itemVM.ProductId, StorageLocationId = defaultLocation.Id, MovementDate = salesReturn.ReturnDate, MovementType = StockMovementType.SalesReturn, Quantity = itemVM.QuantityToReturn, UnitCost = itemVM.UnitPrice, ReferenceDocument = salesReturn.ReturnNumber, UserId = CurrentUserService.LoggedInUser?.Id });
                }

                if (totalReturnValue == 0) throw new Exception("لم يتم تحديد أي كميات للإرجاع.");
                salesReturn.TotalReturnValue = totalReturnValue;
                db.SalesReturns.Add(salesReturn);

                // إنشاء القيد المحاسبي (إشعار دائن)
                var originalSale = await db.Sales.Include(s => s.Customer).FirstOrDefaultAsync(s => s.Id == saleId);
                var creditNoteVoucher = new JournalVoucher { VoucherNumber = $"CN-{salesReturn.ReturnNumber}", VoucherDate = DateTime.Today, Description = $"إشعار دائن للعميل: {originalSale.Customer.CustomerName} لمرتجع من الفاتورة رقم: {originalSale.InvoiceNumber}", TotalDebit = totalReturnValue, TotalCredit = totalReturnValue, Status = VoucherStatus.Posted };
                creditNoteVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultSalesReturnsAccountId.Value, Debit = totalReturnValue, Credit = 0 });
                creditNoteVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsReceivableAccountId.Value, Debit = 0, Credit = totalReturnValue });
                db.JournalVouchers.Add(creditNoteVoucher);

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }

        public async Task PrintCreditNoteAsync(int salesReturnId)
        {
            string tempXpsFile = null;
            try
            {
                using (var db = new DatabaseContext())
                {
                    var salesReturn = await db.SalesReturns.Include(sr => sr.Sale.Customer).Include(sr => sr.SalesReturnItems).ThenInclude(sri => sri.Product).FirstOrDefaultAsync(sr => sr.Id == salesReturnId);
                    if (salesReturn == null) { MessageBox.Show("لم يتم العثور على المرتجع للطباعة."); return; }

                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    var resourceUri = new Uri("/UI/Views/CreditNoteTemplate.xaml", UriKind.Relative);
                    var resource = Application.LoadComponent(resourceUri) as ResourceDictionary;
                    var flowDocument = XamlReader.Parse(XamlWriter.Save(resource["CreditNote"])) as FlowDocument;

                    (flowDocument.FindName("CompanyNameRun") as Run).Text = companyInfo?.CompanyName ?? "اسم الشركة";
                    (flowDocument.FindName("CompanyAddressRun") as Run).Text = companyInfo?.Address ?? "";
                    if (companyInfo?.Logo != null)
                    {
                        var logoImage = new BitmapImage();
                        using (var stream = new MemoryStream(companyInfo.Logo))
                        {
                            logoImage.BeginInit(); logoImage.StreamSource = stream;
                            logoImage.CacheOption = BitmapCacheOption.OnLoad; logoImage.EndInit(); logoImage.Freeze();
                        }
                        (flowDocument.FindName("CompanyLogoImage") as System.Windows.Controls.Image).Source = logoImage;
                    }

                    (flowDocument.FindName("CreditNoteNumberRun") as Run).Text = salesReturn.ReturnNumber;
                    (flowDocument.FindName("CreditNoteDateRun") as Run).Text = salesReturn.ReturnDate.ToString("yyyy/MM/dd");
                    (flowDocument.FindName("OriginalInvoiceRun") as Run).Text = salesReturn.Sale.InvoiceNumber;
                    (flowDocument.FindName("CustomerNameRun") as Run).Text = salesReturn.Sale.Customer.CustomerName;
                    (flowDocument.FindName("CustomerAddressRun") as Run).Text = salesReturn.Sale.Customer.BillingAddress;
                    (flowDocument.FindName("TotalValueRun") as Run).Text = $"{salesReturn.TotalReturnValue:N2}";

                    var itemsTableGroup = (TableRowGroup)flowDocument.FindName("ItemsTableRowGroup");
                    foreach (var item in salesReturn.SalesReturnItems)
                    {
                        var originalSaleItem = await db.SaleItems.AsNoTracking().FirstOrDefaultAsync(si => si.SaleId == salesReturn.SaleId && si.ProductId == item.ProductId);
                        decimal unitPrice = originalSaleItem?.UnitPrice ?? 0;
                        decimal subtotal = item.Quantity * unitPrice;

                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.ProductCode))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.Name))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { TextAlignment = TextAlignment.Center }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{unitPrice:N2}")) { TextAlignment = TextAlignment.Right }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run($"{subtotal:N2}")) { TextAlignment = TextAlignment.Right }));
                        itemsTableGroup.Rows.Add(row);
                    }

                    tempXpsFile = Path.GetTempFileName();
                    using (var xpsDoc = new XpsDocument(tempXpsFile, FileAccess.ReadWrite))
                    {
                        var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                        var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                        writer.Write(paginator);
                        var previewWindow = new PrintPreviewWindow(xpsDoc.GetFixedDocumentSequence(), tempXpsFile);
                        previewWindow.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}", "خطأ");
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempXpsFile) && File.Exists(tempXpsFile))
                {
                    try { File.Delete(tempXpsFile); } catch { }
                }
            }
        }
    }
}