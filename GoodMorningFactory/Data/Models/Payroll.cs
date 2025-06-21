// Data/Models/Payroll.cs
// *** ملف جديد: يمثل مسير رواتب لشهر معين ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class Payroll
    {
        public Payroll()
        {
            Payslips = new HashSet<Payslip>();
        }

        [Key]
        public int Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime DateProcessed { get; set; }
        public decimal TotalAmount { get; set; }

        public virtual ICollection<Payslip> Payslips { get; set; }
    }
}