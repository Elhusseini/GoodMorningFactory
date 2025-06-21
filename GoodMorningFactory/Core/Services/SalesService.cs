// GoodMorningFactory/Core/Services/SalesService.cs
// *** الكود الكامل والشامل والنهائي ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية مسؤولة عن جميع العمليات المتعلقة بالمبيعات،
    /// بما في ذلك جلب الفواتير، إنشاء المرتجعات، وتسجيل الدفعات.
    /// </summary>
    public class SalesService : ISalesService
    {
        public async Task<List<SaleSummaryDto>> GetSalesAsync(string searchText, InvoiceStatus? status, int? customerId, DateTime? dueDateFrom, DateTime? dueDateTo)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Sales.Include(s => s.Customer).Include(s => s.SalesOrder).AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string searchTextLower = searchText.ToLower();
                    query = query.Where(s => s.InvoiceNumber.ToLower().Contains(searchTextLower) ||
                                             (s.SalesOrder != null && s.SalesOrder.SalesOrderNumber.ToLower().Contains(searchTextLower)) ||
                                             (s.Customer != null && s.Customer.CustomerName.ToLower().Contains(searchTextLower)));
                }

                if (status.HasValue)
                {
                    query = query.Where(s => s.Status == status.Value);
                }

                if (customerId.HasValue && customerId.Value > 0)
                {
                    query = query.Where(s => s.CustomerId == customerId.Value);
                }

                if (dueDateFrom.HasValue)
                {
                    query = query.Where(s => s.DueDate >= dueDateFrom.Value.Date);
                }
                if (dueDateTo.HasValue)
                {
                    query = query.Where(s => s.DueDate <= dueDateTo.Value.Date.AddDays(1).AddTicks(-1));
                }

                var salesFromDb = await query.OrderByDescending(s => s.SaleDate).ToListAsync();
                var saleIds = salesFromDb.Select(s => s.Id);

                var returnsDictionary = await db.SalesReturns
                    .Where(sr => saleIds.Contains(sr.SaleId))
                    .GroupBy(sr => sr.SaleId)
                    .Select(g => new { SaleId = g.Key, TotalReturned = g.Sum(sr => sr.TotalReturnValue) })
                    .ToDictionaryAsync(x => x.SaleId, x => x.TotalReturned);

                var result = salesFromDb.Select(sale => new SaleSummaryDto
                {
                    Sale = sale,
                    TotalReturned = returnsDictionary.ContainsKey(sale.Id) ? returnsDictionary[sale.Id] : 0
                }).ToList();

                return result;
            }
        }

        /// <summary>
        /// جلب فاتورة واحدة مع جميع تفاصيلها لغرض الطباعة.
        /// </summary>
        public Sale GetSaleForPrinting(int saleId)
        {
            using (var db = new DatabaseContext())
            {
                return db.Sales
                    .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                    .Include(s => s.Customer)
                    .Include(s => s.SalesOrder)
                    .FirstOrDefault(s => s.Id == saleId);
            }
        }

        /// <summary>
        /// جلب معلومات الشركة الأساسية.
        /// </summary>
        public CompanyInfo GetCompanyInfo()
        {
            using (var db = new DatabaseContext())
            {
                return db.CompanyInfos.FirstOrDefault();
            }
        }

        /// <summary>
        /// جلب بيانات عملة معينة بواسطة معرفها.
        /// </summary>
        public Currency GetCurrency(int currencyId)
        {
            using (var db = new DatabaseContext())
            {
                return db.Currencies.Find(currencyId);
            }
        }

        // --- دوال المرتجعات (كاملة) ---

        /// <summary>
        /// جلب البيانات الكاملة للفاتورة الأصلية وبنودها، لتحضيرها لعملية الإرجاع.
        /// </summary>
        public async Task<Sale> GetDataForReturnCreationAsync(int saleId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Sales
                    .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                    .FirstOrDefaultAsync(s => s.Id == saleId);
            }
        }

        // --- بداية التعديل الرئيسي: استكمال منطق مرتجعات المبيعات ---
        /// <summary>
        /// إنشاء مرتجع مبيعات جديد مع تحديث المخزون وإنشاء قيد محاسبي.
        /// </summary>
        public async Task CreateSalesReturnAsync(int saleId, List<SalesReturnItemViewModel> itemsToReturn)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var itemsToProcess = itemsToReturn.Where(i => i.QuantityToReturn > 0).ToList();
                    if (!itemsToProcess.Any())
                    {
                        throw new InvalidOperationException("لم يتم تحديد أي كميات للإرجاع.");
                    }

                    // التحقق من وجود الحسابات الافتراضية
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultSalesReturnsAccountId == null || companyInfo?.DefaultAccountsReceivableAccountId == null)
                        throw new Exception("يرجى تحديد حساب 'مردودات المبيعات' و 'الذمم المدينة' الافتراضي في الإعدادات.");

                    // الحصول على موقع التخزين الافتراضي
                    var defaultLocation = await db.StorageLocations.FirstOrDefaultAsync(sl => sl.IsDefault);
                    if (defaultLocation == null)
                        throw new Exception("لا يوجد موقع تخزين افتراضي معرف لاستقبال المرتجعات.");

                    var salesReturn = new SalesReturn
                    {
                        ReturnNumber = $"RTN-{DateTime.Now:yyyyMMddHHmmss}",
                        ReturnDate = DateTime.Today,
                        SaleId = saleId,
                    };

                    decimal totalReturnValue = 0;

                    foreach (var item in itemsToProcess)
                    {
                        if (item.QuantityToReturn > (item.OriginalQuantity - item.PreviouslyReturnedQuantity))
                        {
                            throw new InvalidOperationException($"لا يمكن إرجاع كمية أكبر من المسموح بها للمنتج: {item.ProductName}");
                        }

                        salesReturn.SalesReturnItems.Add(new SalesReturnItem { ProductId = item.ProductId, Quantity = item.QuantityToReturn });
                        totalReturnValue += item.QuantityToReturn * item.UnitPrice;

                        // تحديث رصيد المخزون (زيادة الكمية)
                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.StorageLocationId == defaultLocation.Id);
                        if (inventory != null)
                        {
                            inventory.Quantity += item.QuantityToReturn;
                        }
                        else
                        {
                            db.Inventories.Add(new Inventory { ProductId = item.ProductId, StorageLocationId = defaultLocation.Id, Quantity = item.QuantityToReturn });
                        }

                        // تسجيل حركة المخزون
                        var product = await db.Products.FindAsync(item.ProductId);
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = item.ProductId,
                            StorageLocationId = defaultLocation.Id,
                            MovementDate = salesReturn.ReturnDate,
                            MovementType = StockMovementType.SalesReturn,
                            Quantity = item.QuantityToReturn,
                            UnitCost = product?.AverageCost ?? 0, // استخدام متوسط التكلفة
                            ReferenceDocument = salesReturn.ReturnNumber,
                            UserId = CurrentUserService.LoggedInUser?.Id
                        });
                    }

                    salesReturn.TotalReturnValue = totalReturnValue;
                    db.SalesReturns.Add(salesReturn);
                    await db.SaveChangesAsync(); // حفظ المرتجع للحصول على ID

                    // إنشاء القيد المحاسبي (إشعار دائن)
                    var originalSale = await db.Sales.Include(s => s.Customer).FirstOrDefaultAsync(s => s.Id == saleId);
                    var creditNoteVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"CN-{salesReturn.ReturnNumber}",
                        VoucherDate = DateTime.Today,
                        Description = $"إشعار دائن للعميل: {originalSale.Customer.CustomerName} لمرتجع من الفاتورة رقم: {originalSale.InvoiceNumber}",
                        TotalDebit = totalReturnValue,
                        TotalCredit = totalReturnValue,
                        Status = VoucherStatus.Posted
                    };
                    // الطرف المدين: حساب مردودات المبيعات (زيادة المصروف أو تخفيض الإيراد)
                    creditNoteVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultSalesReturnsAccountId.Value, Debit = totalReturnValue, Credit = 0 });
                    // الطرف الدائن: حساب الذمم المدينة (تخفيض رصيد العميل)
                    creditNoteVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsReceivableAccountId.Value, Debit = 0, Credit = totalReturnValue });
                    db.JournalVouchers.Add(creditNoteVoucher);

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        // --- نهاية التعديل الرئيسي ---

        /// <summary>
        /// جلب الكميات التي تم إرجاعها سابقاً من فاتورة معينة.
        /// </summary>
        public async Task<Dictionary<int, int>> GetPreviouslyReturnedQuantitiesAsync(int saleId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.SalesReturnItems
                    .Where(sri => sri.SalesReturn.SaleId == saleId)
                    .GroupBy(sri => sri.ProductId)
                    .Select(g => new { ProductId = g.Key, TotalReturned = g.Sum(i => i.Quantity) })
                    .ToDictionaryAsync(g => g.ProductId, g => g.TotalReturned);
            }
        }

        // --- دوال الدفعات (كاملة مع القيد المحاسبي) ---

        /// <summary>
        /// جلب التفاصيل المالية لفاتورة معينة، بما في ذلك حساب المرتجعات لتحديد الرصيد الصحيح.
        /// </summary>
        public async Task<PaymentDetailsDto> GetPaymentDetailsAsync(int saleId)
        {
            using (var db = new DatabaseContext())
            {
                var sale = await db.Sales.FindAsync(saleId);
                if (sale == null) return null;

                decimal totalReturned = await db.SalesReturns
                    .Where(sr => sr.SaleId == saleId)
                    .SumAsync(sr => (decimal?)sr.TotalReturnValue) ?? 0;

                decimal balanceDue = sale.TotalAmount - totalReturned - sale.AmountPaid;

                return new PaymentDetailsDto
                {
                    InvoiceNumber = sale.InvoiceNumber,
                    TotalAmount = sale.TotalAmount,
                    PreviouslyPaid = sale.AmountPaid,
                    TotalReturned = totalReturned,
                    BalanceDue = balanceDue
                };
            }
        }

        /// <summary>
        /// تسجيل دفعة جديدة وتحديث حالة الفاتورة وإنشاء القيد المحاسبي اللازم.
        /// </summary>
        public async Task RecordPaymentAsync(int saleId, decimal amountPaid)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var sale = await db.Sales.Include(s => s.Customer).FirstOrDefaultAsync(s => s.Id == saleId);
                    if (sale == null) throw new InvalidOperationException("لم يتم العثور على الفاتورة.");

                    // تحديث المبلغ المدفوع في الفاتورة
                    sale.AmountPaid += amountPaid;

                    // جلب إجمالي المرتجعات مرة أخرى هنا لضمان دقة الحساب داخل المعاملة
                    decimal totalReturned = await db.SalesReturns
                                                  .Where(sr => sr.SaleId == saleId)
                                                  .SumAsync(sr => (decimal?)sr.TotalReturnValue) ?? 0;

                    // حساب صافي قيمة الفاتورة
                    decimal netInvoiceAmount = sale.TotalAmount - totalReturned;

                    // المقارنة مع صافي القيمة بدلاً من الإجمالي
                    if (sale.AmountPaid >= netInvoiceAmount)
                    {
                        sale.Status = InvoiceStatus.Paid;
                    }
                    else
                    {
                        sale.Status = InvoiceStatus.PartiallyPaid;
                    }

                    // *** بداية منطق القيد المحاسبي (كامل) ***
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultCashAccountId == null || companyInfo?.DefaultAccountsReceivableAccountId == null)
                    {
                        throw new Exception("لا يمكن إنشاء القيد المحاسبي. يرجى التأكد من تحديد حساب 'النقدية/البنك' و 'الذمم المدينة' الافتراضي في شاشة الإعدادات.");
                    }

                    var journalVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"RCP-{sale.InvoiceNumber}-{DateTime.Now:HHmmss}",
                        VoucherDate = DateTime.Today,
                        Description = $"تحصيل دفعة من العميل: {sale.Customer.CustomerName} للفاتورة رقم: {sale.InvoiceNumber}",
                        TotalDebit = amountPaid,
                        TotalCredit = amountPaid,
                        Status = VoucherStatus.Posted
                    };

                    // الطرف المدين: حساب النقدية/البنك
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultCashAccountId.Value, Debit = amountPaid, Credit = 0 });
                    // الطرف الدائن: حساب الذمم المدينة (العملاء)
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsReceivableAccountId.Value, Debit = 0, Credit = amountPaid });

                    db.JournalVouchers.Add(journalVoucher);
                    // *** نهاية منطق القيد المحاسبي ***

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Sale> GetSaleForEditAsync(int saleId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                    .AsNoTracking() // استخدام AsNoTracking لتجنب مشاكل تتبع الكيانات
                    .FirstOrDefaultAsync(s => s.Id == saleId);
            }
        }

        public async Task UpdateSaleAsync(Sale saleToUpdate, List<SaleItem> newItems, decimal newAmountPaid)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var saleInDb = await db.Sales.Include(s => s.SaleItems).FirstOrDefaultAsync(s => s.Id == saleToUpdate.Id);
                    if (saleInDb == null) throw new Exception("لم يتم العثور على الفاتورة.");

                    // 1. عكس حركات المخزون القديمة
                    foreach (var oldItem in saleInDb.SaleItems)
                    {
                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == oldItem.ProductId);
                        if (inventory != null) inventory.Quantity += oldItem.Quantity;
                    }
                    db.SaleItems.RemoveRange(saleInDb.SaleItems);
                    await db.SaveChangesAsync();

                    // 2. تحديث بيانات الفاتورة الأساسية
                    saleInDb.SaleDate = saleToUpdate.SaleDate;
                    saleInDb.AmountPaid = newAmountPaid;
                    saleInDb.Subtotal = newItems.Sum(i => i.Quantity * i.UnitPrice);
                    saleInDb.TaxAmount = 0; // يمكنك إضافة منطق الضرائب هنا
                    saleInDb.TotalAmount = saleInDb.Subtotal + saleInDb.TaxAmount;

                    // 3. إضافة البنود الجديدة وتحديث المخزون
                    foreach (var newItem in newItems)
                    {
                        saleInDb.SaleItems.Add(newItem);
                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == newItem.ProductId);
                        if (inventory != null && inventory.Quantity >= newItem.Quantity) { inventory.Quantity -= newItem.Quantity; }
                        else
                        {
                            var product = await db.Products.FindAsync(newItem.ProductId);
                            throw new Exception($"الكمية غير كافية في المخزون للمنتج: {product?.Name}");
                        }
                    }

                    // ======================= بداية الإصلاح الرئيسي =======================
                    // 4. تحديث حالة الفاتورة
                    decimal totalReturned = await db.SalesReturns.Where(sr => sr.SaleId == saleInDb.Id).SumAsync(sr => (decimal?)sr.TotalReturnValue) ?? 0;
                    decimal balanceDue = saleInDb.TotalAmount - saleInDb.AmountPaid - totalReturned;
                    if (balanceDue <= 0.01m) { saleInDb.Status = InvoiceStatus.Paid; }
                    else if (saleInDb.AmountPaid > 0) { saleInDb.Status = InvoiceStatus.PartiallyPaid; }
                    else { saleInDb.Status = InvoiceStatus.Sent; }

                    var relatedVoucher = await db.JournalVouchers.Include(v => v.JournalVoucherItems).FirstOrDefaultAsync(v => v.VoucherNumber == $"INV-{saleInDb.InvoiceNumber}");
                    if (relatedVoucher != null)
                    {
                        relatedVoucher.TotalDebit = saleInDb.TotalAmount;
                        relatedVoucher.TotalCredit = saleInDb.TotalAmount;
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception) { await transaction.RollbackAsync(); throw; }
            }
                    }

        public async Task AddSaleAsync(Sale newSale, List<SaleItem> items, decimal amountPaid)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultSalesAccountId == null || companyInfo?.DefaultAccountsReceivableAccountId == null)
                        throw new Exception("يرجى تحديد حساب 'المبيعات' و 'الذمم المدينة' الافتراضي في الإعدادات.");

                    newSale.InvoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}";
                    newSale.AmountPaid = amountPaid;
                    newSale.Subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
                    newSale.TaxAmount = 0;
                    newSale.TotalAmount = newSale.Subtotal + newSale.TaxAmount;

                    if (newSale.TotalAmount <= newSale.AmountPaid) { newSale.Status = InvoiceStatus.Paid; }
                    else if (newSale.AmountPaid > 0) { newSale.Status = InvoiceStatus.PartiallyPaid; }
                    else { newSale.Status = InvoiceStatus.Sent; }

                    db.Sales.Add(newSale);
                    await db.SaveChangesAsync();

                    foreach (var item in items)
                    {
                        item.SaleId = newSale.Id;
                        db.SaleItems.Add(item);
                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                        if (inventory != null && inventory.Quantity >= item.Quantity) { inventory.Quantity -= item.Quantity; }
                    else
                    {
                            var product = await db.Products.FindAsync(item.ProductId);
                            throw new Exception($"الكمية غير كافية في المخزون للمنتج: {product?.Name}");
                    }
                    }

                    // ======================= بداية الإصلاح الرئيسي =======================
                    // جلب اسم العميل قبل استخدامه
                    var customer = await db.Customers.FindAsync(newSale.CustomerId);
                    var journalVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"INV-{newSale.InvoiceNumber}",
                        VoucherDate = newSale.SaleDate,
                        Description = $"فاتورة مبيعات للعميل: {customer?.CustomerName ?? "غير محدد"}",
                        TotalDebit = newSale.TotalAmount,
                        TotalCredit = newSale.TotalAmount,
                        Status = VoucherStatus.Posted
                    };
                    // ======================== نهاية الإصلاح الرئيسي ========================

                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsReceivableAccountId.Value, Debit = newSale.TotalAmount, Credit = 0 });
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultSalesAccountId.Value, Debit = 0, Credit = newSale.TotalAmount });
                    db.JournalVouchers.Add(journalVoucher);

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception) { await transaction.RollbackAsync(); throw; }
                }
            }
        }
}