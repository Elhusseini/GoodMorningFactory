// Core/Services/IHRService.cs
// *** تحديث: تمت إضافة دوال خاصة بالحضور والانصراف ***

using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels; // <-- إضافة جديدة
using System; // <-- إضافة جديدة
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// الواجهة التي تحدد العمليات المتاحة لوحدة الموارد البشرية.
    /// تعمل كعقد بين الـ ViewModel وقاعدة البيانات.
    /// </summary>
    public interface IHRService
    {
        #region Employee Management
        /// <summary>
        /// جلب قائمة بجميع الموظفين مع معلوماتهم الأساسية للعرض.
        /// </summary>
        Task<List<Employee>> GetEmployeesAsync();

        /// <summary>
        /// جلب موظف واحد عن طريق هويته (ID) للتعديل.
        /// </summary>
        Task<Employee> GetEmployeeByIdAsync(int employeeId);

        /// <summary>
        /// حفظ موظف جديد أو تحديث بيانات موظف حالي في قاعدة البيانات.
        /// </summary>
        Task SaveEmployeeAsync(Employee employee);
        #endregion

        #region Leave Type Management
        /// <summary>
        /// جلب قائمة بجميع أنواع الإجازات المعرّفة في النظام.
        /// </summary>
        Task<List<LeaveType>> GetLeaveTypesAsync();

        /// <summary>
        /// جلب نوع إجازة واحد عن طريق هويته (ID).
        /// </summary>
        Task<LeaveType> GetLeaveTypeByIdAsync(int leaveTypeId);

        /// <summary>
        /// حفظ نوع إجازة جديد أو تحديث بيانات نوع حالي.
        /// </summary>
        Task SaveLeaveTypeAsync(LeaveType leaveType);

        /// <summary>
        /// حذف نوع إجازة من قاعدة البيانات.
        /// </summary>
        Task DeleteLeaveTypeAsync(int leaveTypeId);
        #endregion

        #region Leave Request Management
        /// <summary>
        /// جلب قائمة بجميع طلبات الإجازات مع تضمين بيانات الموظف ونوع الإجازة.
        /// </summary>
        Task<List<LeaveRequest>> GetLeaveRequestsAsync();

        /// <summary>
        /// إضافة طلب إجازة جديد إلى قاعدة البيانات.
        /// </summary>
        Task AddLeaveRequestAsync(LeaveRequest leaveRequest);

        /// <summary>
        /// تحديث حالة طلب إجازة (موافقة أو رفض).
        /// </summary>
        Task UpdateLeaveRequestStatusAsync(int leaveRequestId, LeaveRequestStatus newStatus);
        #endregion

        #region Attendance Management (الكود المضاف في هذه المرحلة)
        /// <summary>
        /// جلب ملخص الحضور والانصراف المجمع لكل الموظفين في تاريخ محدد.
        /// </summary>
        Task<List<AttendanceSummaryViewModel>> GetAttendanceSummaryAsync(DateTime date);

        /// <summary>
        /// إضافة سجل حركة (حضور أو انصراف) يدويًا.
        /// </summary>
        Task AddAttendanceRecordAsync(AttendanceRecord record);
        #endregion

        #region Payroll Management (بداية الإضافة الجديدة)
        /// <summary>
        /// جلب قائمة بجميع مسيرات الرواتب السابقة.
        /// </summary>
        Task<List<Payroll>> GetPayrollsAsync();

        /// <summary>
        /// حساب بيانات الرواتب الأولية (قبل الحفظ) للموظفين النشطين لشهر وسنة محددين.
        /// </summary>
        Task<List<PayslipViewModel>> CalculatePayslipsAsync(int year, int month);

        /// <summary>
        /// حفظ مسير الرواتب بالكامل (المسير الرئيسي، تفاصيل الرواتب، والقيد المحاسبي) داخل عملية واحدة.
        /// </summary>
        Task ProcessAndSavePayrollAsync(Payroll payroll, List<PayslipViewModel> payslips);
        #endregion (نهاية الإضافة الجديدة)

    }
}