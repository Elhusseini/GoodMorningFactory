// GoodMorningFactory/Core/Services/IBomService.cs
// *** ملف جديد: واجهة خدمة لوحدة قوائم المكونات (BOM) ***
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IBomService
    {
        /// <summary>
        /// جلب قائمة بجميع قوائم المكونات.
        /// </summary>
        Task<List<BillOfMaterials>> GetBomsAsync();

        /// <summary>
        /// حذف قائمة مكونات معينة.
        /// </summary>
        Task DeleteBomAsync(int bomId);
    }
}