// UI/ViewModels/PayrollViewModel.cs
// *** ملف جديد: ViewModel لواجهة عرض مسيرات الرواتب ***

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PayrollViewModel : BaseViewModel
    {
        private readonly IHRService _hrService;

        public ObservableCollection<Payroll> Payrolls { get; set; } = new ObservableCollection<Payroll>();

        public ICommand LoadPayrollsCommand { get; }
        public ICommand CreatePayrollCommand { get; }

        public PayrollViewModel()
        {
            _hrService = new HRService();
            LoadPayrollsCommand = new AsyncRelayCommand(LoadPayrollsAsync);
            CreatePayrollCommand = new RelayCommand(_ => CreatePayroll());

            // تحميل المسيرات السابقة عند بدء التشغيل
            LoadPayrollsCommand.Execute(null);
        }

        private async Task LoadPayrollsAsync()
        {
            var payrollsList = await _hrService.GetPayrollsAsync();
            Payrolls.Clear();
            foreach (var payroll in payrollsList)
            {
                Payrolls.Add(payroll);
            }
        }

        private void CreatePayroll()
        {
            // إنشاء ViewModel جديد لنافذة المعالجة
            var processViewModel = new ProcessPayrollViewModel(_hrService);
            var processWindow = new ProcessPayrollWindow
            {
                DataContext = processViewModel // ربط النافذة بالـ ViewModel
            };

            // بعد إغلاق نافذة المعالجة، قم بتحديث قائمة المسيرات
            if (processWindow.ShowDialog() == true)
            {
                LoadPayrollsCommand.Execute(null);
            }
        }
    }
}