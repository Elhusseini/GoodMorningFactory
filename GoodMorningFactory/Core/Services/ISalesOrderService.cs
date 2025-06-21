// GoodMorningFactory/Core/Services/ISalesOrderService.cs

using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels; // سيبقى هذا مؤقتاً لـ DTO
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface ISalesOrderService
    {
        // تم تعديل هذه الدالة لتُرجع نموذج البيانات الأصلي
        Task<PaginatedResult<SalesOrder>> GetSalesOrdersAsync(int page, int pageSize, string searchText, OrderStatus? status);

        Task CancelSalesOrderAsync(int orderId);

        // تم تعديل هذه الدالة لتُرجع البيانات اللازمة لإنشاء أوامر العمل
        Task<List<SalesOrderItem>> GetItemsForWorkOrderCreationAsync(int orderId);

        // --- بداية الإضافة: إضافة الدالة المفقودة للواجهة ---
        /// <summary>
        /// تحديث حالة أمر البيع إلى "قيد التنفيذ".
        /// </summary>
        /// <param name="orderId">معرف أمر البيع.</param>
        Task UpdateOrderStatusToInProcessAsync(int orderId);
        // --- نهاية الإضافة ---

        Task<SalesOrder> GetSalesOrderForEditAsync(int orderId);
        Task<SalesQuotation> GetQuotationForConversionAsync(int quotationId);
        Task<Product> FindProductAsync(string searchText);
        Task<decimal> GetProductPriceAsync(int productId, int? priceListId);
        Task SaveSalesOrderAsync(SalesOrder order);
        Task<string> GetNextSalesOrderNumberAsync();
        Task<(bool Exceeded, string Message)> CheckCreditLimitAsync(int customerId, decimal newOrderValue, int? currentOrderId);
        Task<InvoiceCreationDataDto> GetDataForInvoicingAsync(int salesOrderId);
        Task CreateInvoiceFromOrderAsync(int salesOrderId, DateTime invoiceDate, DateTime? dueDate, IEnumerable<InvoiceItemViewModel> itemsToInvoice);
    }

    public class InvoiceCreationDataDto
    {
        public string SalesOrderNumber { get; set; }
        public int PaymentTerms { get; set; }
        public List<InvoiceItemViewModel> InvoiceableItems { get; set; } = new List<InvoiceItemViewModel>();
    }
}