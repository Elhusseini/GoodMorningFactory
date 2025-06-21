using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using LiveCharts; // UPDATED
using LiveCharts.Wpf; // UPDATED
using System.Collections.Generic;
using System.Globalization;
using GoodMorningFactory.Core.Services; // إضافة لاستخدام AppSettings

namespace GoodMorningFactory.UI.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PurchasingDashboardViewModel : ViewModelBase
    {
        private decimal _totalOpenPOValue;
        private int _invoicesDueSoonCount;
        private int _pendingRequisitionsCount;
        private ObservableCollection<RecentPurchaseOrderViewModel> _recentPurchaseOrders;
        private SeriesCollection _series;
        private string[] _labels;

        public decimal TotalOpenPOValue { get => _totalOpenPOValue; set { _totalOpenPOValue = value; OnPropertyChanged(); } }
        public int InvoicesDueSoonCount { get => _invoicesDueSoonCount; set { _invoicesDueSoonCount = value; OnPropertyChanged(); } }
        public int PendingRequisitionsCount { get => _pendingRequisitionsCount; set { _pendingRequisitionsCount = value; OnPropertyChanged(); } }
        public ObservableCollection<RecentPurchaseOrderViewModel> RecentPurchaseOrders { get => _recentPurchaseOrders; set { _recentPurchaseOrders = value; OnPropertyChanged(); } }

        // --- Chart Properties (UPDATED for LiveCharts.Wpf) ---
        public SeriesCollection Series { get => _series; set { _series = value; OnPropertyChanged(); } }
        public string[] Labels { get => _labels; set { _labels = value; OnPropertyChanged(); } }
        public Func<double, string> YFormatter { get; set; }

        public PurchasingDashboardViewModel()
        {
            LoadDashboardData();
        }

        private async void LoadDashboardData()
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    // جلب معلومات الشركة للحصول على رمز العملة الافتراضي
                    var companyInfo = await context.CompanyInfos.Include(ci => ci.DefaultCurrency).FirstOrDefaultAsync();
                    string currencySymbol = companyInfo?.DefaultCurrency?.Symbol ?? "د.ك"; // استخدام رمز العملة الافتراضي أو "د.ك" كافتراضي

                    // --- KPI Data Loading (No changes here) ---
                    TotalOpenPOValue = await context.PurchaseOrders.Where(po => po.Status == PurchaseOrderStatus.Sent || po.Status == PurchaseOrderStatus.Confirmed || po.Status == PurchaseOrderStatus.PartiallyReceived).SumAsync(po => po.TotalAmount);
                    var sevenDaysFromNow = DateTime.Today.AddDays(7);
                    InvoicesDueSoonCount = await context.Purchases.CountAsync(p => p.DueDate <= sevenDaysFromNow && p.Status != PurchaseInvoiceStatus.FullyPaid);
                    PendingRequisitionsCount = await context.PurchaseRequisitions.CountAsync(pr => pr.Status == RequisitionStatus.PendingApproval);
                    var recentPOs = await context.PurchaseOrders.Include(po => po.Supplier).OrderByDescending(po => po.OrderDate).Take(5)
                        .Select(po => new RecentPurchaseOrderViewModel
                        {
                            DocumentNumber = po.PurchaseOrderNumber,
                            SupplierName = po.Supplier.Name,
                            TotalAmount = po.TotalAmount,
                            CurrencySymbol = currencySymbol // تمرير رمز العملة
                        }).ToListAsync();
                    RecentPurchaseOrders = new ObservableCollection<RecentPurchaseOrderViewModel>(recentPOs);

                    // --- Chart Data Loading (UPDATED for LiveCharts.Wpf) ---
                    var sixMonthsAgo = DateTime.Today.AddMonths(-6);
                    var monthlyPurchases = await context.Purchases
                        .Where(p => p.PurchaseDate >= sixMonthsAgo)
                        .GroupBy(p => new { p.PurchaseDate.Year, p.PurchaseDate.Month })
                        .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Total = g.Sum(p => p.TotalAmount) })
                        .OrderBy(g => g.Year).ThenBy(g => g.Month)
                        .ToListAsync();

                    var chartLabels = new List<string>();
                    var chartValues = new ChartValues<decimal>();

                    for (int i = 5; i >= 0; i--)
                    {
                        var date = DateTime.Today.AddMonths(-i);
                        var monthData = monthlyPurchases.FirstOrDefault(m => m.Year == date.Year && m.Month == date.Month);
                        chartLabels.Add(date.ToString("MMM", new CultureInfo("ar-AE"))); // Arabic month name
                        chartValues.Add(monthData?.Total ?? 0);
                    }

                    Series = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "إجمالي المشتريات",
                            Values = chartValues
                        }
                    };

                    Labels = chartLabels.ToArray();
                    // استخدام رمز العملة الافتراضي في YFormatter
                    YFormatter = value => $"{value:N2} {currencySymbol}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load purchasing dashboard data: {ex.Message}");
                // Optionally, show a message to the user
            }
        }
    }

    public class RecentPurchaseOrderViewModel
    {
        public string DocumentNumber { get; set; }
        public string SupplierName { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencySymbol { get; set; } // إضافة خاصية لرمز العملة
        public string TotalAmountFormatted => $"{TotalAmount:N2} {CurrencySymbol}"; // تنسيق المبلغ مع رمز العملة
    }
}