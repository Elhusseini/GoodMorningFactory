// UI/Views/ProductionSchedulingView.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GoodMorningFactory.UI.Views
{
    // ViewModel خاص بمهام مخطط جانت
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

    // محول لتحويل القيمة الرقمية للمسافة اليسرى إلى Thickness
    public class LeftMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness((double)value, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ProductionSchedulingView : UserControl
    {
        private const double DayWidth = 50.0; // عرض كل يوم بالبكسل

        public ProductionSchedulingView()
        {
            InitializeComponent();
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // جلب أوامر العمل المفتوحة فقط
                    var openStatuses = new[] { WorkOrderStatus.Planned, WorkOrderStatus.InProgress, WorkOrderStatus.OnHold };
                    var workOrders = db.WorkOrders
                        .Include(wo => wo.FinishedGood)
                        .Where(wo => openStatuses.Contains(wo.Status))
                        .OrderBy(wo => wo.PlannedStartDate)
                        .ToList();

                    if (!workOrders.Any())
                    {
                        // لا توجد بيانات لعرضها
                        TimelineHeader.ItemsSource = null;
                        GanttChartItems.ItemsSource = null;
                        return;
                    }

                    // تحديد بداية ونهاية المخطط الزمني
                    var timelineStart = workOrders.Min(wo => wo.PlannedStartDate).Date;
                    var timelineEnd = workOrders.Max(wo => wo.PlannedEndDate).Date.AddDays(10); // إضافة أيام إضافية للمساحة

                    // إنشاء رأس المخطط الزمني (قائمة التواريخ)
                    var timelineDates = new List<DateTime>();
                    for (var date = timelineStart; date <= timelineEnd; date = date.AddDays(1))
                    {
                        timelineDates.Add(date);
                    }
                    TimelineHeader.ItemsSource = timelineDates;

                    // إنشاء قائمة مهام جانت
                    var ganttTasks = new List<GanttTaskViewModel>();
                    foreach (var wo in workOrders)
                    {
                        ganttTasks.Add(new GanttTaskViewModel
                        {
                            WorkOrder = wo,
                            TaskName = wo.FinishedGood.Name,
                            StartDate = wo.PlannedStartDate,
                            EndDate = wo.PlannedEndDate,
                            LeftOffset = (wo.PlannedStartDate - timelineStart).TotalDays * DayWidth,
                            BarWidth = ((wo.PlannedEndDate - wo.PlannedStartDate).TotalDays + 1) * DayWidth,
                            BarColor = GetStatusColor(wo.Status)
                        });
                    }
                    GanttChartItems.ItemsSource = ganttTasks;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل جدول الإنتاج: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // دالة مساعدة لتحديد لون الشريط بناءً على حالة أمر العمل
        private Brush GetStatusColor(WorkOrderStatus status)
        {
            switch (status)
            {
                case WorkOrderStatus.InProgress:
                    return new SolidColorBrush(Colors.ForestGreen);
                case WorkOrderStatus.OnHold:
                    return new SolidColorBrush(Colors.OrangeRed);
                case WorkOrderStatus.Planned:
                    return new SolidColorBrush(Colors.CornflowerBlue);
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }
    }
}
