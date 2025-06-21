// GoodMorningFactory/Core/Services/AccountsReceivableService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore; // <-- تأكد من وجود هذا السطر
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class AccountsReceivableService : IAccountsReceivableService
    {
        public async Task<List<AccountsReceivableViewModel>> GetAgingReportAsync()
        {
            using (var db = new DatabaseContext())
            {
                var openInvoices = await db.Sales
                    .Include(s => s.Customer)
                    .Where(s => s.Status != InvoiceStatus.Paid && s.Status != InvoiceStatus.Cancelled)
                    .ToListAsync();

                // --- بداية التحسين: جلب إجمالي المرتجعات لكل الفواتير مرة واحدة ---
                var saleIds = openInvoices.Select(i => i.Id).ToList();
                var returnsTotals = await db.SalesReturns
                    .Where(sr => saleIds.Contains(sr.SaleId))
                    .GroupBy(sr => sr.SaleId)
                    // *** بداية الإصلاح: تم تغيير 'pr' إلى 'sr' ***
                    .Select(g => new { SaleId = g.Key, TotalReturned = g.Sum(sr => sr.TotalReturnValue) })
                    // *** نهاية الإصلاح ***
                    .ToDictionaryAsync(x => x.SaleId, x => x.TotalReturned);
                // --- نهاية التحسين ---

                var arList = new List<AccountsReceivableViewModel>();
                DateTime today = DateTime.Today;

                foreach (var invoice in openInvoices)
                {
                    // --- بداية التعديل: استخدام القاموس بدلاً من الاستعلام المتكرر ---
                    decimal totalReturnedValue = returnsTotals.ContainsKey(invoice.Id) ? returnsTotals[invoice.Id] : 0;
                    var balanceDue = invoice.TotalAmount - invoice.AmountPaid - totalReturnedValue;
                    // --- نهاية التعديل ---

                    if (balanceDue <= 0) continue;

                    var dueDate = invoice.DueDate ?? invoice.SaleDate.AddDays(30);
                    var daysOverdue = (today - dueDate).Days;

                    var vm = new AccountsReceivableViewModel
                    {
                        SaleId = invoice.Id,
                        CustomerName = invoice.Customer?.CustomerName ?? "N/A",
                        InvoiceNumber = invoice.InvoiceNumber,
                        DueDate = dueDate,
                        TotalAmount = invoice.TotalAmount,
                        BalanceDue = balanceDue
                    };

                    if (daysOverdue <= 30) vm.Bucket0_30 = balanceDue;
                    else if (daysOverdue <= 60) vm.Bucket31_60 = balanceDue;
                    else if (daysOverdue <= 90) vm.Bucket61_90 = balanceDue;
                    else vm.BucketOver90 = balanceDue;

                    arList.Add(vm);
                }
                return arList.OrderBy(a => a.CustomerName).ThenBy(a => a.DueDate).ToList();
            }
        }
    }
}