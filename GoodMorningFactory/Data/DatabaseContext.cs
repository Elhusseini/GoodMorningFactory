// Data/DatabaseContext.cs
// تم تعديل هذا الملف بشكل جوهري لإضافة منطق تسجيل التدقيق تلقائياً

using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Core.Services; // <-- إضافة مهمة للوصول إلى المستخدم الحالي
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // <-- إضافة مهمة لتتبع التغييرات
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json; // <-- إضافة مهمة لتحويل التغييرات إلى نص
using System.Threading.Tasks;

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
        public DbSet<Currency> Currencies { get; set; } // إضافة جدول العملات
        public DbSet<TaxRule> TaxRules { get; set; }
        public DbSet<WorkOrderLaborLog> WorkOrderLaborLogs { get; set; }
        public DbSet<Department> Departments { get; set; } // إضافة جدول الأقسام
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<StockAdjustmentItem> StockAdjustmentItems { get; set; }

        public DbSet<AccountingPeriod> AccountingPeriods { get; set; }
        public DbSet<FixedAsset> FixedAssets { get; set; }
        public DbSet<CostCenter> CostCenters { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<BudgetDetail> BudgetDetails { get; set; }
        public DbSet<StockTransfer> StockTransfers { get; set; }
        public DbSet<StockTransferItem> StockTransferItems { get; set; }
        public DbSet<StorageLocation> StorageLocations { get; set; } // ** إضافة جديدة **
        public DbSet<AuditLog> AuditLogs { get; set; } // <-- إضافة DbSet جديد لسجل التدقيق

        // --- بداية الإضافة ---
        public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; }
        public DbSet<ApprovalWorkflowStep> ApprovalWorkflowSteps { get; set; }
        public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
        // --- نهاية الإضافة ---
        // --- بداية الإضافة ---
        public DbSet<SerialNumber> SerialNumbers { get; set; }
        public DbSet<LotNumber> LotNumbers { get; set; }
        // --- بداية الإضافة ---
        public DbSet<Lead> Leads { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        // --- نهاية الإضافة ---

        // --- بداية الإضافة: تعريف الجداول الجديدة ---
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<InventoryCount> InventoryCounts { get; set; }
        public DbSet<InventoryCountItem> InventoryCountItems { get; set; }
        // --- نهاية الإضافة ---


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
            modelBuilder.Entity<StockAdjustment>().HasIndex(sa => sa.ReferenceNumber).IsUnique();

            // فهرس فريد للفترة المحاسبية (لا يمكن تكرار نفس الشهر والسنة)
            modelBuilder.Entity<AccountingPeriod>().HasIndex(p => new { p.Year, p.Month }).IsUnique();


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

            modelBuilder.Entity<CompanyInfo>()
                .HasOne(c => c.DefaultCurrency)
                .WithMany()
                .HasForeignKey(c => c.DefaultCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- بداية التحديث: تطبيق القواعد على كلاس Account ---
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .HasMany(a => a.ChildAccounts)
                .WithOne(a => a.ParentAccount)
                .HasForeignKey(a => a.ParentAccountId);
            // ------------------------- نهاية التحديث ----
            // --- بداية الإضافة: إضافة بيانات أولية للعملات ---
            // هذا الكود سيضمن وجود هذه العملات في قاعدة البيانات عند إنشائها لأول مرة
            modelBuilder.Entity<Currency>().HasData(
                new Currency { Id = 1, Name = "دينار كويتي", Code = "KWD", Symbol = "د.ك", IsActive = true },
                new Currency { Id = 2, Name = "دولار أمريكي", Code = "USD", Symbol = "$", IsActive = true },
                new Currency { Id = 3, Name = "يورو", Code = "EUR", Symbol = "€", IsActive = true },
                new Currency { Id = 4, Name = "جنيه مصر", Code = "EGP", Symbol = "ج.م", IsActive = true }
            );

            // --- بداية الإضافة (إذا لم يكن موجوداً) ---
            modelBuilder.Entity<NumberingSequence>()
                .Property(e => e.DocumentType)
                .HasConversion<string>();
            // --- نهاية الإضافة ---

            modelBuilder.Entity<StockAdjustment>().HasIndex(sa => sa.ReferenceNumber).IsUnique(); // إضافة فهرس فريد

            // --- الإضافة الجديدة ---
            modelBuilder.Entity<Budget>().HasIndex(b => new { b.Name, b.Year }).IsUnique();
            modelBuilder.Entity<BudgetDetail>().HasIndex(bd => new { bd.BudgetId, bd.AccountId }).IsUnique();

            // --- الإضافة الجديدة ---
            modelBuilder.Entity<StockTransfer>().HasIndex(st => st.TransferNumber).IsUnique();

            // --- بداية التعديل: تحديث علاقات التحويلات المخزنية ---

            modelBuilder.Entity<StockTransfer>()
            .HasOne(st => st.SourceStorageLocation)
            .WithMany()
            .HasForeignKey(st => st.SourceStorageLocationId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransfer>()
                .HasOne(st => st.DestinationStorageLocation)
                .WithMany()
                .HasForeignKey(st => st.DestinationStorageLocationId)
                .OnDelete(DeleteBehavior.Restrict);
            // --- نهاية التعديل ---

            // --- بداية الإضافة: تعريف فهارس فريدة للجداول الجديدة ---
            modelBuilder.Entity<SerialNumber>()
                .HasIndex(sn => new { sn.ProductId, sn.Value })
                .IsUnique();

            modelBuilder.Entity<LotNumber>()
                .HasIndex(ln => new { ln.ProductId, ln.Value, ln.StorageLocationId })
                .IsUnique();
            // --- نهاية الإضافة ---


            modelBuilder.Entity<StorageLocation>()
               .HasIndex(sl => new { sl.WarehouseId, sl.Code }).IsUnique();


            // --- بداية الإضافة: تعريف الفهارس والعلاقات للجداول الجديدة ---
            modelBuilder.Entity<StockMovement>()
                .Property(e => e.MovementType)
                .HasConversion<string>(); // تخزين الـ enum كنص لسهولة القراءة

            modelBuilder.Entity<InventoryCount>()
                .HasIndex(ic => ic.CountReferenceNumber)
                .IsUnique();
            // --- نهاية الإضافة ---

        }


        // --- بداية الإضافة:  (override) لدالة الحفظ ---
        // تم عمل override لدالة SaveChangesAsync لتعتراض جميع عمليات الحفظ
        // وإنشاء سجلات تدقيق قبل إتمام عملية الحفظ في قاعدة البيانات.
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default)
        {
            // 1. جلب قائمة بالتغييرات التي سيتم تسجيلها
            var auditEntries = CreateAuditEntries();

            // 2. حفظ التغييرات الأصلية (على المنتجات، الفواتير، إلخ)
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            // 3. إذا تمت التغييرات الأصلية بنجاح، قم بحفظ سجلات التدقيق
            if (auditEntries.Any())
            {
                this.AuditLogs.AddRange(auditEntries);
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        // دالة مساعدة لإنشاء سجلات التدقيق بناءً على التغييرات المتعقبة
        private List<AuditLog> CreateAuditEntries()
        {
            var auditEntries = new List<AuditLog>();

            // المرور على كل التغييرات التي اكتشفها Entity Framework
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            {
                // تجاهل أي تغييرات على جدول سجلات التدقيق نفسه لمنع حلقة لا نهائية
                if (entry.Entity is AuditLog)
                    continue;

                var auditEntry = new AuditLog
                {
                    ActionType = entry.State switch
                    {
                        EntityState.Added => AuditActionType.Create,
                        EntityState.Modified => AuditActionType.Update,
                        EntityState.Deleted => AuditActionType.Delete,
                        _ => throw new NotSupportedException()
                    },
                    EntityName = entry.Metadata.GetTableName(),
                    Timestamp = DateTime.UtcNow,
                    UserId = CurrentUserService.LoggedInUser?.Id,
                    Username = CurrentUserService.LoggedInUser?.Username ?? "System"
                };

                // الحصول على المفتاح الأساسي للسجل
                auditEntry.EntityKey = GetPrimaryKey(entry);

                var changes = new Dictionary<string, object>();

                // في حالة التعديل، سجل القيم القديمة والجديدة
                if (entry.State == EntityState.Modified)
                {
                    foreach (var property in entry.Properties)
                    {
                        if (property.IsModified)
                        {
                            changes[property.Metadata.Name] = new
                            {
                                Old = property.OriginalValue?.ToString(),
                                New = property.CurrentValue?.ToString()
                            };
                        }
                    }
                }
                // في حالة الإضافة، سجل جميع القيم الجديدة
                else if (entry.State == EntityState.Added)
                {
                    foreach (var property in entry.Properties)
                    {
                        changes[property.Metadata.Name] = property.CurrentValue?.ToString();
                    }
                }

                auditEntry.Changes = JsonSerializer.Serialize(changes);
                auditEntries.Add(auditEntry);
            }
            return auditEntries;
        }

        // دالة مساعدة لجلب قيمة المفتاح الأساسي للسجل
        private string GetPrimaryKey(EntityEntry entry)
        {
            var key = entry.Metadata.FindPrimaryKey();
            var keyValues = key.Properties.Select(p => entry.Property(p.Name).CurrentValue);
            return string.Join(", ", keyValues);
        }
        // --- نهاية الإضافة ---
    }
}