// GoodMorningFactory/Core/Services/IInventoryCountService.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العقد الخاص بخدمات أوامر الجرد.
    /// </summary>
    public interface IInventoryCountService
    {
        /// <summary>
        /// جلب قائمة بأوامر الجرد مع دعم للفلترة والترقيم.
        /// </summary>
        Task<PaginatedResult<InventoryCount>> GetInventoryCountsAsync(InventoryCountFilterCriteria criteria);

        /// <summary>
        /// جلب البيانات الأساسية اللازمة لنافذة إضافة/تعديل أمر جرد.
        /// </summary>
        Task<AddEditInventoryCountDto> GetInitialDataForCountWindowAsync(int? countId);

        /// <summary>
        /// جلب قائمة المنتجات الموجودة في مخزن معين مع كمياتها الحالية.
        /// </summary>
        Task<List<InventoryCountItemViewModel>> GetProductsForWarehouseAsync(int warehouseId);

        /// <summary>
        /// حفظ أمر الجرد (كمسودة أو قيد التنفيذ).
        /// </summary>
        Task<int> SaveInventoryCountAsync(InventoryCount count, List<InventoryCountItemViewModel> items);

        /// <summary>
        /// ترحيل الفروقات النهائية لأمر الجرد.
        /// </summary>
        Task PostInventoryCountAsync(int countId);

        /// <summary>
        /// إلغاء أمر جرد.
        /// </summary>
        Task CancelInventoryCountAsync(int countId);
    }

    /// <summary>
    /// كلاس مساعد لتمرير معايير الفلترة.
    /// </summary>
    public class InventoryCountFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SearchText { get; set; }
        public InventoryCountStatus? Status { get; set; }
    }

    /// <summary>
    /// كائن لنقل البيانات المبدئية إلى ViewModel نافذة الإضافة/التعديل.
    /// </summary>
    public class AddEditInventoryCountDto
    {
        public InventoryCount InventoryCount { get; set; }
        public List<InventoryCountItemViewModel> Items { get; set; }
        public List<Warehouse> Warehouses { get; set; }
        public List<User> Users { get; set; }

        public AddEditInventoryCountDto()
        {
            Items = new List<InventoryCountItemViewModel>();
        }
    }
}