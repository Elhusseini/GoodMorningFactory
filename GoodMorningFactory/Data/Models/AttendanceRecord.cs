// Data/Models/AttendanceRecord.cs
// *** ملف جديد: يمثل سجل حركة حضور أو انصراف واحد ***
using System;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public enum RecordType { In, Out }

    public class AttendanceRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } // تاريخ ووقت الحركة

        [Required]
        public RecordType RecordType { get; set; } // نوع الحركة (حضور أو انصراف)

        public string? Notes { get; set; }
    }
}