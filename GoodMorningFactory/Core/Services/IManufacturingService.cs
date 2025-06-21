// Services/IManufacturingService.cs
// الواجهة التي تحدد العمليات الخاصة بوحدة التصنيع
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Services
{
    public interface IManufacturingService
    {
        /// <summary>
        /// جلب قائمة بالمنتجات النهائية
        /// </summary>
        Task<List<Product>> GetFinishedGoodsAsync();

        /// <summary>
        /// جلب قائمة بالمواد الخام
        /// </summary>
        Task<List<Product>> GetRawMaterialsAsync();

        /// <summary>
        /// البحث عن مادة خام باستخدام كود أو اسم المنتج
        /// </summary>
        Task<Product> FindRawMaterialAsync(string searchTerm);

        /// <summary>
        /// جلب بيانات قائمة مكونات (BOM) للتعديل
        /// </summary>
        Task<BillOfMaterials> GetBomByIdAsync(int bomId);

        /// <summary>
        /// حفظ (إضافة أو تعديل) قائمة المكونات
        /// </summary>
        Task SaveBomAsync(BillOfMaterials bom);
    }
}