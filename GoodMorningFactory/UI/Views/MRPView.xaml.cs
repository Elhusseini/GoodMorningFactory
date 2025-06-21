// UI/Views/MRPView.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class MRPView : UserControl
    {
        public MRPView()
        {
            InitializeComponent();
        }

        private void RunMrpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var openSalesOrders = db.SalesOrders
                        .Include(so => so.SalesOrderItems)
                            .ThenInclude(soi => soi.Product)
                        .Where(so => so.Status != OrderStatus.Shipped && so.Status != OrderStatus.Invoiced && so.Status != OrderStatus.Cancelled)
                        .ToList();

                    var grossRequirements = new Dictionary<int, decimal>();

                    foreach (var order in openSalesOrders)
                    {
                        foreach (var item in order.SalesOrderItems)
                        {
                            if (item.Product.ProductType != ProductType.FinishedGood && item.Product.ProductType != ProductType.WorkInProgress)
                                continue;

                            var bom = db.BillOfMaterials
                                .Include(b => b.BillOfMaterialsItems)
                                .FirstOrDefault(b => b.FinishedGoodId == item.ProductId);

                            if (bom != null)
                            {
                                foreach (var material in bom.BillOfMaterialsItems)
                                {
                                    decimal requiredQty = material.Quantity * item.Quantity;
                                    if (grossRequirements.ContainsKey(material.RawMaterialId))
                                    {
                                        grossRequirements[material.RawMaterialId] += requiredQty;
                                    }
                                    else
                                    {
                                        grossRequirements[material.RawMaterialId] = requiredQty;
                                    }
                                }
                            }
                        }
                    }

                    var onHandInventory = db.Inventories.ToDictionary(i => i.ProductId, i => i.Quantity);
                    var scheduledReceipts = db.PurchaseOrderItems
                        .Where(poi => poi.PurchaseOrder.Status != PurchaseOrderStatus.FullyReceived && poi.PurchaseOrder.Status != PurchaseOrderStatus.Cancelled)
                        .GroupBy(poi => poi.ProductId)
                        .ToDictionary(g => g.Key, g => g.Sum(item => item.Quantity));

                    var mrpResults = new List<MRPResultViewModel>();
                    var allRawMaterials = db.Products.Where(p => grossRequirements.Keys.Contains(p.Id)).Include(p => p.UnitOfMeasure).ToList();

                    foreach (var material in allRawMaterials)
                    {
                        decimal grossReq = grossRequirements.ContainsKey(material.Id) ? grossRequirements[material.Id] : 0;
                        int onHand = onHandInventory.ContainsKey(material.Id) ? onHandInventory[material.Id] : 0;
                        int scheduled = scheduledReceipts.ContainsKey(material.Id) ? scheduledReceipts[material.Id] : 0;

                        decimal netReq = grossReq - (onHand + scheduled);

                        mrpResults.Add(new MRPResultViewModel
                        {
                            ProductId = material.Id,
                            ProductCode = material.ProductCode,
                            ProductName = material.Name,
                            UnitOfMeasure = material.UnitOfMeasure?.Name ?? "N/A",
                            GrossRequirements = grossReq,
                            OnHandInventory = onHand,
                            ScheduledReceipts = scheduled,
                            NetRequirements = netReq > 0 ? netReq : 0
                        });
                    }

                    MrpResultsDataGrid.ItemsSource = mrpResults.Where(r => r.NetRequirements > 0).OrderBy(r => r.ProductName).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حساب المتطلبات: {ex.Message}", "خطأ فادح", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- بداية التعديل: تفعيل منطق زر إنشاء طلب الشراء ---
        private void CreateRequisitionButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is MRPResultViewModel selectedMaterial)
            {
                if (selectedMaterial.NetRequirements <= 0) return;

                // استدعاء المُنشئ الجديد وتمرير البيانات إليه
                var requisitionWindow = new AddEditPurchaseRequisitionWindow(selectedMaterial.ProductId, selectedMaterial.NetRequirements);
                requisitionWindow.ShowDialog();

                // يمكنك إضافة منطق لتحديث شاشة MRP بعد إغلاق نافذة الطلب إذا لزم الأمر
            }
        }
        // --- نهاية التعديل ---
    }
}
