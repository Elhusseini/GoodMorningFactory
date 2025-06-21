using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    // كلاس مساعد لتمرير معايير الفلترة والترقيم لخدمة الشحنات.
    /// </summary>
    public class ShipmentFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string SearchText { get; set; }
        public ShipmentStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
