// GoodMorningFactory/Core/Services/IPurchaseReturnService.cs
// *** الكود الكامل والمعدل ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IPurchaseReturnService
    {
        Task<List<PurchaseReturn>> GetPurchaseReturnsAsync();
        Task<List<Purchase>> GetReturnablePurchasesAsync();
        Task<Purchase> GetPurchaseDetailsForReturnAsync(int purchaseId);

        // ======================= بداية الإضافة =======================
        /// <summary>
        /// جلب الكميات المرتجعة سابقاً لفاتورة معينة.
        /// </summary>
        Task<Dictionary<int, int>> GetReturnedItemsForPurchaseAsync(int purchaseId);
        // ======================== نهاية الإضافة ========================

        Task CreatePurchaseReturnAsync(int purchaseId, IEnumerable<PurchaseReturnItemViewModel> itemsToReturn);
    }
}