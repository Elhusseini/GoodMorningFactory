// GoodMorningFactory/Core/Services/AccountsPayableService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class AccountsPayableService : IAccountsPayableService
    {
        public async Task<List<AccountsPayableViewModel>> GetAgingReportAsync()
        {
            using (var db = new DatabaseContext())
            {
                var openInvoices = await db.Purchases
                    .Include(p => p.Supplier)
                    .Where(p => p.Status != PurchaseInvoiceStatus.FullyPaid && p.Status != PurchaseInvoiceStatus.Cancelled)
                    .ToListAsync();

                // --- بداية الإضافة: جلب إجمالي المرتجعات لكل فاتورة مرة واحدة ---
                var purchaseIds = openInvoices.Select(i => i.Id).ToList();
                var returnsTotals = await db.PurchaseReturns
                    .Where(pr => purchaseIds.Contains(pr.PurchaseId))
                    .GroupBy(pr => pr.PurchaseId)
                    .Select(g => new { PurchaseId = g.Key, TotalReturned = g.Sum(sr => sr.TotalReturnValue) })
                    .ToDictionaryAsync(x => x.PurchaseId, x => x.TotalReturned);
                // --- نهاية الإضافة ---

                var apList = new List<AccountsPayableViewModel>();
                DateTime today = DateTime.Today;

                foreach (var invoice in openInvoices)
                {
                    // --- بداية التعديل: حساب الرصيد المستحق بشكل دقيق ---
                    decimal totalReturnedValue = returnsTotals.ContainsKey(invoice.Id) ? returnsTotals[invoice.Id] : 0;
                    var balanceDue = invoice.TotalAmount - invoice.AmountPaid - totalReturnedValue;
                    // --- نهاية التعديل ---

                    if (balanceDue <= 0) continue;

                    DateTime dueDate = invoice.DueDate ?? invoice.PurchaseDate.AddDays(30);
                    var daysOverdue = (today - dueDate).Days;

                    var vm = new AccountsPayableViewModel
                    {
                        PurchaseId = invoice.Id,
                        SupplierName = invoice.Supplier.Name,
                        InvoiceNumber = invoice.InvoiceNumber,
                        DueDate = dueDate,
                        TotalAmount = invoice.TotalAmount,
                        BalanceDue = balanceDue
                    };

                    if (daysOverdue <= 0) vm.Bucket0_30 = balanceDue; // Not due yet or due within 30 days
                    else if (daysOverdue <= 30) vm.Bucket0_30 = balanceDue;
                    else if (daysOverdue <= 60) vm.Bucket31_60 = balanceDue;
                    else if (daysOverdue <= 90) vm.Bucket61_90 = balanceDue;
                    else vm.BucketOver90 = balanceDue;

                    apList.Add(vm);
                }
                return apList.OrderBy(a => a.SupplierName).ThenBy(a => a.DueDate).ToList();
            }
        }

        public async Task<PaymentDetailsDto> GetPaymentDetailsAsync(int purchaseId)
        {
            using (var db = new DatabaseContext())
            {
                var purchase = await db.Purchases.FindAsync(purchaseId);
                if (purchase == null) return null;

                // --- بداية التعديل: إضافة حساب المرتجعات هنا أيضاً ---
                decimal totalReturned = await db.PurchaseReturns
                                               .Where(pr => pr.PurchaseId == purchaseId)
                                               .SumAsync(pr => (decimal?)pr.TotalReturnValue) ?? 0;
                // --- نهاية التعديل ---

                return new PaymentDetailsDto
                {
                    InvoiceNumber = purchase.InvoiceNumber,
                    TotalAmount = purchase.TotalAmount,
                    PreviouslyPaid = purchase.AmountPaid,
                    TotalReturned = totalReturned, // <-- إضافة قيمة المرتجعات
                    BalanceDue = purchase.TotalAmount - purchase.AmountPaid - totalReturned // <-- حساب الرصيد الصحيح
                };
            }
        }

        // --- بداية التعديل الرئيسي: استكمال منطق تسجيل الدفعة ---
        /// <summary>
        /// تسجيل دفعة لفاتورة مورد مع إنشاء قيد محاسبي تلقائي.
        /// </summary>
        public async Task RecordPaymentAsync(int purchaseId, decimal amountPaid)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    // التحقق من وجود المستخدم الحالي
                    if (CurrentUserService.LoggedInUser == null)
                        throw new InvalidOperationException("المستخدم الحالي غير معروف.");

                    // جلب الفاتورة مع المورد لتضمين اسمه في وصف القيد
                    var purchase = await db.Purchases.Include(p => p.Supplier).FirstOrDefaultAsync(p => p.Id == purchaseId);
                    if (purchase == null)
                        throw new KeyNotFoundException("لم يتم العثور على فاتورة الشراء.");

                    // تحديث المبلغ المدفوع في الفاتورة
                    purchase.AmountPaid += amountPaid;

                    // جلب إجمالي المرتجعات مرة أخرى لضمان دقة الحساب
                    decimal totalReturned = await db.PurchaseReturns
                                                  .Where(pr => pr.PurchaseId == purchaseId)
                                                  .SumAsync(pr => (decimal?)pr.TotalReturnValue) ?? 0;

                    decimal netAmount = purchase.TotalAmount - totalReturned;

                    // تحديث حالة الفاتورة
                    purchase.Status = (purchase.AmountPaid >= netAmount)
                        ? PurchaseInvoiceStatus.FullyPaid
                        : PurchaseInvoiceStatus.PartiallyPaid;

                    // جلب الحسابات الافتراضية من الإعدادات
                    var companyInfo = await db.CompanyInfos.FirstOrDefaultAsync();
                    if (companyInfo?.DefaultCashAccountId == null || companyInfo?.DefaultAccountsPayableAccountId == null)
                        throw new InvalidOperationException("يرجى تحديد حساب 'النقدية/البنك' و 'الذمم الدائنة' الافتراضي في الإعدادات.");

                    // إنشاء القيد المحاسبي
                    var journalVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"PAY-{purchase.InvoiceNumber}-{DateTime.Now:HHmmss}",
                        VoucherDate = DateTime.Now,
                        Description = $"سداد دفعة للمورد: {purchase.Supplier.Name} للفاتورة رقم: {purchase.InvoiceNumber}",
                        TotalDebit = amountPaid,
                        TotalCredit = amountPaid,
                        Status = VoucherStatus.Posted
                    };

                    // الطرف المدين: حساب الذمم الدائنة (تخفيض الالتزام)
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsPayableAccountId.Value, Debit = amountPaid, Credit = 0 });
                    // الطرف الدائن: حساب النقدية/البنك (تخفيض الأصل)
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultCashAccountId.Value, Debit = 0, Credit = amountPaid });

                    db.JournalVouchers.Add(journalVoucher);

                    await db.SaveChangesAsync(true); // *** بداية الإصلاح: إضافة true ***
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
    }
}