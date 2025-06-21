// GoodMorningFactory/Core/Services/CustomerService.cs

// --- ملاحظة: هذا الكلاس هو التطبيق الفعلي للواجهة ICustomerService. ---
// --- يحتوي على كل المنطق البرمجي الخاص بالتعامل مع قاعدة البيانات للعملاء. ---
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
    /// <summary>
    /// خدمة مركزية لإدارة جميع العمليات المتعلقة بالعملاء.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        // --- (كل الدوال الأخرى لديك تبقى كما هي دون أي تغيير) ---
        public async Task<int> AddCustomerAsync(Customer customer)
        {
            using (var db = new DatabaseContext())
            {
                await db.Customers.AddAsync(customer);
                await db.SaveChangesAsync();
                return customer.Id;
            }
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            using (var db = new DatabaseContext())
            {
                db.Customers.Update(customer);
                await db.SaveChangesAsync();
            }
        }

        public async Task<PaginatedResult<Customer>> GetCustomersAsync(CustomerFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Customers.AsQueryable();

                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string searchTextLower = criteria.SearchText.ToLower();
                    query = query.Where(c => c.CustomerName.ToLower().Contains(searchTextLower) || c.CustomerCode.ToLower().Contains(searchTextLower));
                }

                if (criteria.IsActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == criteria.IsActive.Value);
                }

                int totalItems = await query.CountAsync();

                var customers = await query.OrderBy(c => c.CustomerName)
                                           .Skip((criteria.Page - 1) * criteria.PageSize)
                                           .Take(criteria.PageSize)
                                           .ToListAsync();

                return new PaginatedResult<Customer>
                {
                    Items = customers,
                    TotalCount = totalItems
                };
            }
        }

        public async Task<Dictionary<int, decimal>> GetCustomerBalancesAsync(IEnumerable<int> customerIds)
        {
            using (var db = new DatabaseContext())
            {
                var salesAggregates = await db.Sales
                    .Where(s => customerIds.Contains(s.CustomerId) && s.Status != InvoiceStatus.Cancelled)
                    .GroupBy(s => s.CustomerId)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        TotalInvoiced = g.Sum(s => s.TotalAmount),
                        TotalPaid = g.Sum(s => s.AmountPaid)
                    })
                    .ToDictionaryAsync(x => x.CustomerId);

                var returnsAggregates = await db.SalesReturns
                    .Where(sr => customerIds.Contains(sr.Sale.CustomerId))
                    .GroupBy(sr => sr.Sale.CustomerId)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        TotalReturned = g.Sum(sr => sr.TotalReturnValue)
                    })
                    .ToDictionaryAsync(x => x.CustomerId);

                var balances = new Dictionary<int, decimal>();
                foreach (var id in customerIds)
                {
                    decimal totalInvoiced = salesAggregates.ContainsKey(id) ? salesAggregates[id].TotalInvoiced : 0;
                    decimal totalPaid = salesAggregates.ContainsKey(id) ? salesAggregates[id].TotalPaid : 0;
                    decimal totalReturned = returnsAggregates.ContainsKey(id) ? returnsAggregates[id].TotalReturned : 0;
                    balances[id] = totalInvoiced - totalPaid - totalReturned;
                }

                return balances;
            }
        }

        public async Task<Customer> GetCustomerByIdAsync(int customerId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Customers.FindAsync(customerId);
            }
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            using (var db = new DatabaseContext())
            {
                if (await db.Sales.AnyAsync(s => s.CustomerId == customerId))
                {
                    throw new InvalidOperationException("لا يمكن حذف هذا العميل لوجود فواتير مبيعات مرتبطة به.");
                }

                var customer = await db.Customers.FindAsync(customerId);
                if (customer != null)
                {
                    db.Customers.Remove(customer);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<List<Customer>> GetActiveCustomersAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Customers.Where(c => c.IsActive).ToListAsync();
            }
        }

        /// <summary>
        /// *** بداية الإصلاح النهائي ***
        /// تم تعديل هذه الدالة لتشمل "مرتجعات المبيعات" كحركة دائنة مستقلة في كشف الحساب.
        /// </summary>
        public async Task<List<CustomerStatementItemViewModel>> GetCustomerStatementAsync(int customerId)
        {
            using (var db = new DatabaseContext())
            {
                // جلب الفواتير كحركات مدينة
                var invoices = await db.Sales
                    .Where(s => s.CustomerId == customerId && s.Status != InvoiceStatus.Cancelled)
                    .Select(s => new CustomerStatementItemViewModel
                    {
                        Date = s.SaleDate,
                        TransactionType = "فاتورة مبيعات",
                        ReferenceNumber = s.InvoiceNumber,
                        Debit = s.TotalAmount,
                        Credit = 0
                    }).ToListAsync();

                // جلب المدفوعات كحركات دائنة
                var payments = await db.Sales
                    .Where(s => s.CustomerId == customerId && s.Status != InvoiceStatus.Cancelled && s.AmountPaid > 0)
                    .Select(s => new CustomerStatementItemViewModel
                    {
                        Date = s.SaleDate,
                        TransactionType = "دفعة",
                        ReferenceNumber = s.InvoiceNumber,
                        Debit = 0,
                        Credit = s.AmountPaid
                    }).ToListAsync();

                // *** الإضافة الجديدة: جلب المرتجعات كحركات دائنة ***
                var returns = await db.SalesReturns
                    .Where(sr => sr.Sale.CustomerId == customerId)
                    .Select(sr => new CustomerStatementItemViewModel
                    {
                        Date = sr.ReturnDate,
                        TransactionType = "مرتجع مبيعات",
                        ReferenceNumber = sr.ReturnNumber,
                        Debit = 0,
                        Credit = sr.TotalReturnValue
                    }).ToListAsync();

                // دمج جميع الحركات (فواتير، دفعات، مرتجعات) وترتيبها حسب التاريخ
                var allTransactions = invoices
                    .Concat(payments)
                    .Concat(returns) // <-- تم إضافة المرتجعات هنا
                    .OrderBy(t => t.Date)
                    .ToList();

                // حساب الرصيد المرحل
                decimal runningBalance = 0;
                foreach (var item in allTransactions)
                {
                    runningBalance += item.Debit - item.Credit;
                    item.Balance = runningBalance;
                }

                return allTransactions;
            }
        }

        // --- بداية الإضافة: تنفيذ دالة توليد الكود ---
        /// <summary>
        /// هذه الدالة تقوم بالبحث في قاعدة البيانات عن آخر عميل تم تسجيله
        /// ثم تأخذ الـ ID الخاص به، تزيد عليه واحد، وتنسق الرقم الجديد.
        /// </summary>
        public async Task<string> GetNextCustomerCodeAsync()
        {
            using (var db = new DatabaseContext())
            {
                // ابحث عن العميل صاحب أعلى Id
                var lastCustomer = await db.Customers.OrderByDescending(c => c.Id).FirstOrDefaultAsync();

                // إذا كان هناك عملاء، خذ الـ Id الخاص بآخر واحد وزد عليه 1. إذا لم يكن هناك، ابدأ من 1.
                int nextId = (lastCustomer?.Id ?? 0) + 1;

                // تنسيق الرقم ليكون دائماً 5 خانات مع أصفار بادئة (مثال: 1 -> 00001)
                string sequentialNumber = nextId.ToString("D5");

                return $"CUST-{sequentialNumber}";
            }
        }
        // --- نهاية الإضافة ---
    }
}