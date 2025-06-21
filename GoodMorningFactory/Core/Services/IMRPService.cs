// Core/Services/IMRPService.cs
// *** ملف جديد: واجهة خدمة تخطيط متطلبات المواد (MRP) ***

using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// الواجهة التي تحدد العقد الخاص بكل عمليات تخطيط متطلبات المواد.
    /// </summary>
    public interface IMRPService
    {
        /// <summary>
        /// يقوم بتشغيل عملية حساب MRP الكاملة بناءً على الطلبات المفتوحة والمخزون الحالي.
        /// </summary>
        /// <returns>قائمة بنتائج MRP للمواد التي تحتاج إلى شراء.</returns>
        Task<List<MRPResultViewModel>> RunMRPAsync();
    }
}