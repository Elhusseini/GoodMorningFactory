using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IProductHistoryService
    {
        Task<Product> GetProductByIdAsync(int productId);
        // --- بداية التعديل: تغيير نوع الإرجاع ---
        // الدالة الآن سترجع قائمة من الـ ViewModel الجاهز للعرض مباشرة
        Task<List<PurchaseHistoryViewModel>> GetPurchaseHistoryForProductAsync(int productId);
        // --- نهاية التعديل ---
    }
}