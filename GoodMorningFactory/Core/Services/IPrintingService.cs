// GoodMorningFactory/Core/Services/IPrintingService.cs
using GoodMorningFactory.Data.Models;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.Core.Services
{
    public interface IPrintingService
    {
        void PrintVisual(FrameworkElement visual, string description);
        Task PrintPackingSlipAsync(Shipment shipment);
        Task PrintCustomerStatementAsync(int customerId);
        Task PrintSalesInvoiceAsync(int saleId);
        Task PrintSalesQuotationAsync(int quotationId);
        Task PrintPurchaseInvoiceAsync(int purchaseId);
        Task PrintPurchaseOrderAsync(int purchaseOrderId);

        // --- بداية الإضافة: إعادة تعريف دالة طباعة سند الاستلام ---
        /// <summary>
        /// يجهز ويطبع سند استلام بضاعة.
        /// </summary>
        /// <param name="grnId">معرف سند الاستلام المطلوب طباعته.</param>
        Task PrintGoodsReceiptNoteAsync(int grnId);
        // --- نهاية الإضافة ---

        /// <summary>
        /// طباعة تقرير قائمة مكونات المنتج (BOM).
        /// </summary>
        Task PrintBomAsync(int bomId);
    }
}