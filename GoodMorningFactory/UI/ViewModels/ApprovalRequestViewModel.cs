// UI/ViewModels/ApprovalRequestViewModel.cs
// ملف جديد: ViewModel لعرض طلبات الموافقة في شاشة "موافقاتي"

using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ApprovalRequestViewModel
    {
        public int ApprovalRequestId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string RequesterName { get; set; }
        public DateTime RequestDate { get; set; }
        public string CurrentStepName { get; set; }
        public ApprovalStatus Status { get; set; }
    }
}
