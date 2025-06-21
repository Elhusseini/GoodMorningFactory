// GoodMorningFactory/UI/ViewModels/ProductionSchedulingViewModel.cs
// *** ملف جديد: ViewModel لواجهة جدولة الإنتاج (مخطط جانت) ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ProductionSchedulingViewModel : BaseViewModel
    {
        private readonly IProductionSchedulingService _schedulingService;
        private const double DayWidth = 50.0; // عرض كل يوم بالبكسل

        public ObservableCollection<DateTime> TimelineDates { get; } = new ObservableCollection<DateTime>();
        public ObservableCollection<GanttTaskViewModel> GanttTasks { get; } = new ObservableCollection<GanttTaskViewModel>();

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ProductionSchedulingViewModel()
        {
            _schedulingService = new ProductionSchedulingService();
            LoadScheduleAsync();
        }

        private async void LoadScheduleAsync()
        {
            IsLoading = true;
            try
            {
                var workOrders = await _schedulingService.GetOpenWorkOrdersAsync();

                GanttTasks.Clear();
                TimelineDates.Clear();

                if (!workOrders.Any()) return;

                // تحديد بداية ونهاية المخطط الزمني
                var timelineStart = workOrders.Min(wo => wo.PlannedStartDate).Date;
                var timelineEnd = workOrders.Max(wo => wo.PlannedEndDate).Date.AddDays(10); // إضافة أيام للمساحة

                // إنشاء رأس المخطط الزمني (قائمة التواريخ)
                for (var date = timelineStart; date <= timelineEnd; date = date.AddDays(1))
                {
                    TimelineDates.Add(date);
                }

                // إنشاء مهام جانت مع حساب العرض والموقع
                foreach (var wo in workOrders)
                {
                    GanttTasks.Add(new GanttTaskViewModel
                    {
                        WorkOrder = wo,
                        TaskName = $"({wo.WorkOrderNumber}) {wo.FinishedGood.Name}",
                        StartDate = wo.PlannedStartDate,
                        EndDate = wo.PlannedEndDate,
                        LeftOffset = (wo.PlannedStartDate.Date - timelineStart).TotalDays * DayWidth,
                        BarWidth = Math.Max(DayWidth, (wo.PlannedEndDate.Date - wo.PlannedStartDate.Date).TotalDays + 1) * DayWidth,
                        BarColor = GetStatusColor(wo.Status)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل جدول الإنتاج: {ex.Message}", "خطأ");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Brush GetStatusColor(WorkOrderStatus status)
        {
            switch (status)
            {
                case WorkOrderStatus.InProgress: return new SolidColorBrush(Colors.ForestGreen);
                case WorkOrderStatus.OnHold: return new SolidColorBrush(Colors.OrangeRed);
                case WorkOrderStatus.Planned: return new SolidColorBrush(Colors.CornflowerBlue);
                default: return new SolidColorBrush(Colors.Gray);
            }
        }
    }

    // ViewModel مساعد لتمثيل مهمة واحدة (شريط) في مخطط جانت
    public class GanttTaskViewModel
    {
        public WorkOrder WorkOrder { get; set; }
        public string TaskName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Duration => (EndDate - StartDate).TotalDays + 1;
        public double LeftOffset { get; set; }
        public double BarWidth { get; set; }
        public Brush BarColor { get; set; }
    }
}