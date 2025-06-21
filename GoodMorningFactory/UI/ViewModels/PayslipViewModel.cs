// UI/ViewModels/PayslipViewModel.cs
// *** تحديث: تمت إضافة خاصية لحساب أيام الغياب ***
namespace GoodMorningFactory.UI.ViewModels
{
    public class PayslipViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public double UnpaidLeaveDays { get; set; }

        // --- بداية الإضافة ---
        public double AbsenceDays { get; set; } // عدد أيام الغياب
        // --- نهاية الإضافة ---

        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
    }
}
