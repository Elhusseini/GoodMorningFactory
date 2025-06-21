// Core/Services/PermissionSeeder.cs
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// فئة مركزية لتعريف جميع صلاحيات النظام.
    /// هذا يسهل إدارتها وتحديثها من مكان واحد.
    /// </summary>
    public static class PermissionSeeder
    {
        public static List<Permission> GetAllPermissions()
        {
            return new List<Permission>
            {
                // ==================== وحدة البيانات الرئيسية ====================
                new Permission { Module = "البيانات الرئيسية", Name = "MainData.View", Description = "عرض شاشات البيانات الرئيسية" },
                new Permission { Module = "البيانات الرئيسية", Name = "MainData.Products.Create", Description = "إضافة وتكرار المنتجات" },
                new Permission { Module = "البيانات الرئيسية", Name = "MainData.Products.Edit", Description = "تعديل المنتجات" },
                new Permission { Module = "البيانات الرئيسية", Name = "MainData.Products.Delete", Description = "حذف المنتجات" },
                new Permission { Module = "البيانات الرئيسية", Name = "MainData.Customers.Full", Description = "إدارة العملاء (إضافة، تعديل، حذف)" },
                // --- بداية الإضافة ---
                new Permission { Module = "CRM", Name = "CRM.View", Description = "عرض وحدة إدارة علاقات العملاء" },
                new Permission { Module = "CRM", Name = "CRM.Leads.Manage", Description = "إدارة العملاء المحتملين (إضافة، تعديل، حذف)" },
                new Permission { Module = "CRM", Name = "CRM.Leads.Convert", Description = "تحويل العملاء المحتملين إلى عملاء دائمين" },
            // --- نهاية الإضافة ---
                new Permission { Module = "البيانات الرئيسية", Name = "MainData.Suppliers.Full", Description = "إدارة الموردين (إضافة، تعديل، حذف)" },

                // ==================== وحدة المبيعات ====================
                new Permission { Module = "المبيعات", Name = "Sales.View", Description = "عرض شاشات المبيعات" },
                new Permission { Module = "المبيعات", Name = "Sales.Quotations.Create", Description = "إنشاء عروض أسعار" },
                new Permission { Module = "المبيعات", Name = "Sales.Quotations.Edit", Description = "تعديل عروض الأسعار" },
                new Permission { Module = "المبيعات", Name = "Sales.Orders.Create", Description = "إنشاء أوامر بيع" },
                new Permission { Module = "المبيعات", Name = "Sales.Orders.Cancel", Description = "إلغاء أوامر البيع" },
                new Permission { Module = "المبيعات", Name = "Sales.Shipments.Create", Description = "إنشاء شحنات وفواتير" },
                new Permission { Module = "المبيعات", Name = "Sales.Returns.Create", Description = "إنشاء مرتجعات مبيعات" },
                new Permission { Module = "المبيعات", Name = "Sales.Payments.Record", Description = "تسجيل دفعات العملاء" },
                new Permission { Module = "المبيعات", Name = "Sales.OverrideCreditLimit", Description = "السماح بتجاوز حد الائتمان للعملاء" },

                // ==================== وحدة المشتريات ====================
                new Permission { Module = "المشتريات", Name = "Purchasing.View", Description = "عرض شاشات المشتريات" },
                new Permission { Module = "المشتريات", Name = "Purchasing.Requisitions.Create", Description = "إنشاء طلبات شراء" },
                new Permission { Module = "المشتريات", Name = "Purchasing.Requisitions.Approve", Description = "الموافقة أو رفض طلبات الشراء" },
                new Permission { Module = "المشتريات", Name = "Purchasing.Orders.Create", Description = "إنشاء أوامر شراء" },
                new Permission { Module = "المشتريات", Name = "Purchasing.Receipts.Create", Description = "تسجيل استلام البضائع" },
                new Permission { Module = "المشتريات", Name = "Purchasing.Invoices.Create", Description = "تسجيل فواتير الموردين" },

                // ==================== وحدة التصنيع ====================
                new Permission { Module = "التصنيع", Name = "Manufacturing.View", Description = "عرض شاشات التصنيع" },
                new Permission { Module = "التصنيع", Name = "Manufacturing.BOM.Full", Description = "إدارة قوائم المكونات (إضافة، تعديل، حذف)" },
                new Permission { Module = "التصنيع", Name = "Manufacturing.WorkOrders.Create", Description = "إنشاء أوامر عمل" },
                new Permission { Module = "التصنيع", Name = "Manufacturing.WorkOrders.Manage", Description = "إدارة أوامر العمل (بدء، إلغاء، تسجيل إنتاج)" },
                new Permission { Module = "التصنيع", Name = "Manufacturing.OverrideConsumption", Description = "السماح بصرف كميات مواد إضافية" },

               // ==================== وحدة المخزون ====================
                new Permission { Module = "المخزون", Name = "Inventory.View", Description = "عرض شاشات المخزون" },
                new Permission { Module = "المخزون", Name = "Inventory.Adjust", Description = "إجراء جرد وتعديل للمخزون" },
                new Permission { Module = "المخزون", Name = "Inventory.Transfers.Create", Description = "إنشاء تحويلات مخزنية" },
                new Permission { Module = "المخزون", Name = "Inventory.LowStock.View", Description = "عرض تنبيهات انخفاض المخزون" },
                new Permission { Module = "المخزون", Name = "Inventory.Locations.Manage", Description = "إدارة مواقع التخزين الفرعية" },
                
                // --- بداية الإضافة ---
                new Permission { Module = "المخزون", Name = "Inventory.Counts.View", Description = "عرض شاشة أوامر الجرد" },
                new Permission { Module = "المخزون", Name = "Inventory.Counts.Manage", Description = "إدارة أوامر الجرد (إضافة, تعديل, ترحيل)" },
                // --- نهاية الإضافة ---




                // ==================== وحدة الموارد البشرية ====================
                new Permission { Module = "الموارد البشرية", Name = "HR.View", Description = "عرض شاشات الموارد البشرية" },
                new Permission { Module = "الموارد البشرية", Name = "HR.Employees.Full", Description = "إدارة الموظفين (إضافة، تعديل)" },
                new Permission { Module = "الموارد البشرية", Name = "HR.Attendance.Manage", Description = "إدارة سجلات الحضور والانصراف" },
                new Permission { Module = "الموارد البشرية", Name = "HR.Leaves.Manage", Description = "إدارة طلبات الإجازات" },
                new Permission { Module = "الموارد البشرية", Name = "HR.Payroll.Process", Description = "احتساب ومعالجة مسير الرواتب" },

                // ==================== وحدة الحسابات ====================
                new Permission { Module = "الحسابات", Name = "Financials.View", Description = "عرض شاشات الحسابات" },
                new Permission { Module = "الحسابات", Name = "Financials.ChartOfAccounts.Manage", Description = "إدارة شجرة الحسابات" },
                new Permission { Module = "الحسابات", Name = "Financials.Vouchers.Create", Description = "إنشاء قيود يومية يدوية" },
                new Permission { Module = "الحسابات", Name = "Financials.Vouchers.Reverse", Description = "عكس قيود اليومية" },
                new Permission { Module = "الحسابات", Name = "Financials.Periods.Close", Description = "إغلاق الفترات المحاسبية" },
                new Permission { Module = "الحسابات", Name = "Financials.Assets.Manage", Description = "إدارة الأصول الثابتة" },
                new Permission { Module = "الحسابات", Name = "Financials.Assets.RunDepreciation", Description = "تشغيل واحتساب الإهلاك" },
                new Permission { Module = "الحسابات", Name = "Financials.Budgets.Manage", Description = "إدارة الموازنات التقديرية" },



                // ==================== وحدة التقارير ====================
                new Permission { Module = "التقارير", Name = "Reports.View", Description = "عرض جميع التقارير" },
                new Permission { Module = "التقارير", Name = "Reports.Export", Description = "تصدير التقارير (PDF, Excel)" },

               // ==================== وحدة الأمان والإعدادات ====================
                new Permission { Module = "الأمان والإعدادات", Name = "Admin.Settings.Manage", Description = "التحكم في جميع إعدادات النظام" },
                new Permission { Module = "الأمان والإعدادات", Name = "Admin.Users.Manage", Description = "إدارة المستخدمين والأدوار والصلاحيات" },
                new Permission { Module = "الأمان والإعدادات", Name = "Admin.Backup.Manage", Description = "التحكم في النسخ الاحتياطي والاستعادة" },
                new Permission { Module = "الأمان والإعدادات", Name = "Admin.AuditTrail.View", Description = "عرض سجل تدقيق النظام" },
                // --- بداية الإضافة ---
                new Permission { Module = "الأمان والإعدادات", Name = "Admin.ApprovalWorkflows.Manage", Description = "إدارة دورات الموافقات" },
                // --- نهاية الإضافة ---


                // ==================== صلاحيات إضافية لضمان ظهور جميع القوائم ====================
                new Permission { Module = "الإعدادات", Name = "Settings.View", Description = "عرض شاشة الإعدادات" },
                new Permission { Module = "الأمان", Name = "Security.View", Description = "عرض شاشة الأمان والصلاحيات" },
                new Permission { Module = "المشتريات", Name = "Purchases.View", Description = "عرض شاشة المشتريات (متوافقة مع القائمة)" },
            };
        }
    }
}