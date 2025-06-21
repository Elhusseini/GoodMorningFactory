// Data/Models/LeaveRequest.cs
// *** ملف جديد: يمثل طلب إجازة واحد لموظف ***
using System;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public enum LeaveRequestStatus { Pending, Approved, Rejected, Cancelled }

    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }
        public virtual LeaveType LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string? Reason { get; set; }

        [Required]
        public LeaveRequestStatus Status { get; set; }
    }
}