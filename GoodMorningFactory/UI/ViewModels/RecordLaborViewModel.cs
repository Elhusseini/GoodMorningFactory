// GoodMorningFactory/UI/ViewModels/RecordLaborViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class RecordLaborViewModel : BaseViewModel
    {
        private readonly IWorkOrderService _workOrderService;
        private readonly int _workOrderId;

        #region الخصائص (Properties)
        private string _workOrderNumber;
        public string WorkOrderNumber { get => _workOrderNumber; set { _workOrderNumber = value; OnPropertyChanged(); } }

        public ObservableCollection<Employee> Employees { get; set; }

        private Employee _selectedEmployee;
        public Employee SelectedEmployee { get => _selectedEmployee; set { _selectedEmployee = value; OnPropertyChanged(); } }

        private DateTime _workDate = DateTime.Today;
        public DateTime WorkDate { get => _workDate; set { _workDate = value; OnPropertyChanged(); } }

        private decimal _hoursWorked;
        public decimal HoursWorked { get => _hoursWorked; set { _hoursWorked = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
        #endregion

        #region الأوامر (Commands)
        public ICommand SaveCommand { get; }
        #endregion

        public RecordLaborViewModel(int workOrderId)
        {
            _workOrderService = new WorkOrderService();
            _workOrderId = workOrderId;
            Employees = new ObservableCollection<Employee>();

            SaveCommand = new RelayCommand(Save, CanSave);

            LoadDataAsync();
        }

        /// <summary>
        /// تحميل البيانات الأولية اللازمة لعرضها في نافذة تسجيل العمالة.
        /// </summary>
        private async void LoadDataAsync()
        {
            try
            {
                var data = await _workOrderService.GetDataForLaborRecordAsync(_workOrderId);
                if (data != null)
                {
                    WorkOrderNumber = data.WorkOrderNumber;
                    foreach (var emp in data.ActiveEmployees)
                    {
                        Employees.Add(emp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        /// <summary>
        /// شرط يحدد ما إذا كان يمكن حفظ سجل العمالة.
        /// </summary>
        private bool CanSave(object parameter)
        {
            return SelectedEmployee != null && WorkDate != null && HoursWorked > 0;
        }

        /// <summary>
        /// يقوم بتنفيذ عملية حفظ سجل العمالة عبر استدعاء الخدمة المختصة.
        /// </summary>
        private async void Save(object parameter)
        {
            try
            {
                await _workOrderService.RecordLaborAsync(_workOrderId, SelectedEmployee.Id, WorkDate, HoursWorked, Description);
                MessageBox.Show("تم تسجيل وقت العمالة بنجاح.", "نجاح");

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الحفظ: {ex.Message}", "خطأ");
            }
        }
    }
}