// GoodMorningFactory/Core/Services/ISalesService.cs
// *** الكود الكامل والشامل والنهائي ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface ISalesService
    {
        // --- الدوال الموجودة أصلاً ---
        /// تم تغيير الدالة لتصبح غير متزامنة وتُرجع كائن مخصص يحتوي على الفاتورة وقيمة المرتجعات.

        Task<List<SaleSummaryDto>> GetSalesAsync(string searchText, InvoiceStatus? status, int? customerId, DateTime? dueDateFrom, DateTime? dueDateTo);
        Sale GetSaleForPrinting(int saleId);
        CompanyInfo GetCompanyInfo();
        Currency GetCurrency(int currencyId);
        Task<Sale> GetDataForReturnCreationAsync(int saleId);
        Task CreateSalesReturnAsync(int saleId, List<SalesReturnItemViewModel> itemsToReturn);
        Task<Dictionary<int, int>> GetPreviouslyReturnedQuantitiesAsync(int saleId);

        // --- الدوال الجديدة التي أضفناها ---
        Task<PaymentDetailsDto> GetPaymentDetailsAsync(int saleId);
        Task RecordPaymentAsync(int saleId, decimal amountPaid);

        // ======================= بداية الإضافة =======================
        /// <summary>
        /// جلب البيانات الكاملة لفاتورة معينة لغرض التعديل.
        /// </summary>
        Task<Sale> GetSaleForEditAsync(int saleId);

        /// <summary>
        /// تحديث فاتورة موجودة مع تحديث المخزون والقيود المحاسبية.
        /// </summary>
        Task UpdateSaleAsync(Sale saleToUpdate, List<SaleItem> newItems, decimal newAmountPaid);

        /// <summary>
        /// إضافة فاتورة جديدة مع تحديث المخزون وإنشاء قيد محاسبي.
        /// </summary>
        Task AddSaleAsync(Sale newSale, List<SaleItem> items, decimal amountPaid);
        // ======================== نهاية الإضافة ========================

    }

    /// <summary>
    /// *** بداية الإصلاح: إضافة تعريف الكلاس المفقود ***
    /// كائن مساعد لنقل بيانات ملخص الفاتورة من الخدمة إلى ViewModel.
    /// </summary>
    public class SaleSummaryDto
    {
        public Sale Sale { get; set; }
        public decimal TotalReturned { get; set; }
    }


}
