// Data/DatabaseContext.cs
// *** الكود الكامل لملف سياق قاعدة البيانات مع إضافة جدول المخازن ***
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace GoodMorningFactory.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() { Database.EnsureCreated(); }

        // جميع جداول قاعدة البيانات
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; } // جدول الربط بين الأدوار والصلاحيات
        public DbSet<Permission> Permissions { get; set; } // جدول الصلاحيات
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<CompanyInfo> CompanyInfos { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<SalesQuotation> SalesQuotations { get; set; }
        public DbSet<SalesQuotationItem> SalesQuotationItems { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentItem> ShipmentItems { get; set; }
        public DbSet<SalesReturn> SalesReturns { get; set; }
        public DbSet<SalesReturnItem> SalesReturnItems { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<JournalVoucher> JournalVouchers { get; set; }
        public DbSet<JournalVoucherItem> JournalVoucherItems { get; set; }
        public DbSet<BillOfMaterials> BillOfMaterials { get; set; }
        public DbSet<BillOfMaterialsItem> BillOfMaterialsItems { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<WorkOrderMaterial> WorkOrderMaterials { get; set; }
        public DbSet<WorkOrderScrap> WorkOrderScraps { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; } // إضافة جدول طلبات الشراء
        public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; } // إضافة جدول وحدات القياس
        public DbSet<PriceList> PriceLists { get; set; } // إضافة جدول قوائم الأسعار
        public DbSet<ProductPrice> ProductPrices { get; set; } // إضافة جدول أسعار المنتجات
        public DbSet<PurchaseRequisitionItem> PurchaseRequisitionItems { get; set; } // إضافة جدول بنود طلبات الشراء
        public DbSet<GoodsReceiptNote> GoodsReceiptNotes { get; set; } // إضافة جدول مذكرات استلام البضائع
        public DbSet<GoodsReceiptNoteItem> GoodsReceiptNoteItems { get; set; } // إضافة جدول بنود مذكرات استلام البضائع
        public DbSet<PurchaseReturn> PurchaseReturns { get; set; } // إضافة جدول مرتجعات المشتريات
        public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; } // إضافة جدول بنود المرتجعات
        public DbSet<BankReconciliation> BankReconciliations { get; set; } // إضافة جدول التسويات البنكية
        public DbSet<Warehouse> Warehouses { get; set; } // إضافة جدول المخازن
        public DbSet<Employee> Employees { get; set; } // إضافة جدول الموظفين
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; } // إضافة جدول الحضور
        public DbSet<LeaveType> LeaveTypes { get; set; } // إضافة جدول أنواع الإجازات
        public DbSet<LeaveRequest> LeaveRequests { get; set; } // إضافة جدول طلبات الإجازات
        public DbSet<Payroll> Payrolls { get; set; } // إضافة جدول مسيرات الرواتب
        public DbSet<Payslip> Payslips { get; set; } // إضافة جدول قسائم الرواتب
        public DbSet<NumberingSequence> NumberingSequences { get; set; } // <-- إضافة جديدة
        public DbSet<NotificationSetting> NotificationSettings { get; set; } // <-- إضافة جديدة



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDataFolder = Path.Combine(documentsPath, "GoodMorningFactory");
            Directory.CreateDirectory(appDataFolder);
            string dbPath = Path.Combine(appDataFolder, "GoodMorningFactory.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // هذا الكود يتم تنفيذه مرة واحدة فقط عند إنشاء قاعدة البيانات
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "مسؤول النظام" },
                new Role { Id = 2, Name = "مدير مبيعات" },
                new Role { Id = 3, Name = "موظف مخزن" }
            );

            // تعريف الفهارس الفريدة لضمان عدم تكرار البيانات
            modelBuilder.Entity<Product>().HasIndex(p => p.ProductCode).IsUnique();
            modelBuilder.Entity<Customer>().HasIndex(c => c.CustomerCode).IsUnique();
            modelBuilder.Entity<Supplier>().HasIndex(s => s.SupplierCode).IsUnique(); // <-- إضافة فهرس فريد
            modelBuilder.Entity<SalesQuotation>().HasIndex(sq => sq.QuotationNumber).IsUnique();
            modelBuilder.Entity<SalesOrder>().HasIndex(so => so.SalesOrderNumber).IsUnique();
            modelBuilder.Entity<Shipment>().HasIndex(s => s.ShipmentNumber).IsUnique();
            modelBuilder.Entity<SalesReturn>().HasIndex(sr => sr.ReturnNumber).IsUnique();
            modelBuilder.Entity<Account>().HasIndex(a => a.AccountNumber).IsUnique();
            modelBuilder.Entity<JournalVoucher>().HasIndex(jv => jv.VoucherNumber).IsUnique();
            modelBuilder.Entity<WorkOrder>().HasIndex(wo => wo.WorkOrderNumber).IsUnique();
            modelBuilder.Entity<PurchaseOrder>().HasIndex(po => po.PurchaseOrderNumber).IsUnique();
            modelBuilder.Entity<GoodsReceiptNote>().HasIndex(grn => grn.GRNNumber).IsUnique();
            modelBuilder.Entity<PurchaseReturn>().HasIndex(pr => pr.ReturnNumber).IsUnique(); // إضافة فهرس فريد للمرتجع
            modelBuilder.Entity<PurchaseRequisition>().HasIndex(pr => pr.RequisitionNumber).IsUnique(); // إضافة فهرس فريد لطلب الشراء
            modelBuilder.Entity<Warehouse>().HasIndex(w => w.Code).IsUnique(); // إضافة فهرس فريد لكود المخزن
            modelBuilder.Entity<Employee>().HasIndex(e => e.EmployeeCode).IsUnique(); // إضافة فهرس فريد لكود الموظف
            modelBuilder.Entity<Employee>().HasIndex(e => e.EmployeeCode).IsUnique();



            // تعريف العلاقات بين الجداول
            modelBuilder.Entity<SaleItem>().HasOne(si => si.Sale).WithMany(s => s.SaleItems).HasForeignKey(si => si.SaleId);
            modelBuilder.Entity<PurchaseItem>().HasOne(pi => pi.Purchase).WithMany(p => p.PurchaseItems).HasForeignKey(pi => pi.PurchaseId);
            modelBuilder.Entity<JournalVoucherItem>().HasOne(jvi => jvi.JournalVoucher).WithMany(jv => jv.JournalVoucherItems).HasForeignKey(jvi => jvi.JournalVoucherId);

            // --- بداية التحديث ---
            // تعريف مفتاح مركب لجدول الربط
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // تعريف العلاقات
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
            // --- نهاية التحديث ---
        }
    }
}