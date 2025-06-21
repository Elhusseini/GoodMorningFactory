// UI/Views/ProductionSchedulingView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة جدولة الإنتاج ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public class SchedulingViewModel : WorkOrder
    {
        // خاصية إضافية لتحديد ما إذا كان الأمر متأخراً
        public bool IsDelayed => (Status == WorkOrderStatus.Planned || Status == WorkOrderStatus.InProgress) && DateTime.Today > PlannedEndDate;
    }

    public partial class ProductionSchedulingView : UserControl
    {
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
                        .Select(wo => new SchedulingViewModel
                        {
                            // نسخ الخصائص من كائن أمر العمل الأصلي
                            Id = wo.Id,
                            WorkOrderNumber = wo.WorkOrderNumber,
                            FinishedGood = wo.FinishedGood,
                            QuantityToProduce = wo.QuantityToProduce,
                            QuantityProduced = wo.QuantityProduced,
                            PlannedStartDate = wo.PlannedStartDate,
                            PlannedEndDate = wo.PlannedEndDate,
                            Status = wo.Status,
                        })
                        .ToList();

                    SchedulingDataGrid.ItemsSource = workOrders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل جدول الإنتاج: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
