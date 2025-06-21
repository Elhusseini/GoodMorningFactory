// Core/Services/SettingsService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _appDataFolder;
        private readonly string _dbPath;
        private readonly string _backupFolder;

        public SettingsService()
        {
            _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GoodMorningFactory");
            _dbPath = Path.Combine(_appDataFolder, "GoodMorningFactory.db");
            _backupFolder = Path.Combine(_appDataFolder, "Backups");
            Directory.CreateDirectory(_backupFolder);
        }

        private async Task<CompanyInfo> GetOrCreateCompanyInfoAsync(DatabaseContext db)
        {
            var info = await db.CompanyInfos.FirstOrDefaultAsync();
            if (info == null)
            {
                info = new CompanyInfo();
                db.CompanyInfos.Add(info);
                await db.SaveChangesAsync(); // الحفظ هنا يضمن وجود ID للكيان الجديد
            }
            return info;
        }

        public async Task<CompanyInfo> GetCompanyInfoAsync()
        {
            using (var db = new DatabaseContext())
            {
                // استخدام AsNoTracking لتحسين الأداء عند القراءة فقط
                return await db.CompanyInfos.AsNoTracking().FirstOrDefaultAsync() ?? new CompanyInfo();
            }
        }

        // استخدام دالة واحدة مرنة للحفظ لتجنب تكرار الكود
        private async Task SaveInfoAsync(Action<CompanyInfo> updateAction)
        {
            using (var db = new DatabaseContext())
            {
                var info = await GetOrCreateCompanyInfoAsync(db);
                updateAction(info);
                await db.SaveChangesAsync();
            }
        }

        public async Task SaveCompanyInfoAsync(CompanyInfo info)
        {
            await SaveInfoAsync(dbInfo =>
            {
                dbInfo.CompanyName = info.CompanyName;
                dbInfo.Address = info.Address;
                dbInfo.City = info.City;
                dbInfo.Country = info.Country;
                dbInfo.PhoneNumber = info.PhoneNumber;
                dbInfo.Email = info.Email;
                dbInfo.Website = info.Website;
                dbInfo.TaxNumber = info.TaxNumber;
                dbInfo.CommercialRegistrationNumber = info.CommercialRegistrationNumber;
                dbInfo.Logo = info.Logo;
            });
        }

        public async Task SaveGeneralSettingsAsync(CompanyInfo info)
        {
            await SaveInfoAsync(dbInfo =>
            {
                dbInfo.DefaultLanguage = info.DefaultLanguage;
                dbInfo.DefaultDateFormat = info.DefaultDateFormat;
                dbInfo.DefaultCurrencyId = info.DefaultCurrencyId;
            });
            // تحديث العملة الافتراضية بشكل منفصل
            using (var db = new DatabaseContext())
            {
                if (info.DefaultCurrencyId.HasValue)
                {
                    var allCurrencies = await db.Currencies.ToListAsync();
                    foreach (var currency in allCurrencies)
                    {
                        currency.IsDefault = (currency.Id == info.DefaultCurrencyId.Value);
                    }
                    await db.SaveChangesAsync();
                }
            }
            AppSettings.LoadSettings(); // إعادة تحميل الإعدادات العامة للتطبيق
        }

        public async Task SaveUserSettingsAsync(CompanyInfo info)
        {
            await SaveInfoAsync(dbInfo =>
            {
                dbInfo.MinPasswordLength = info.MinPasswordLength;
                dbInfo.PasswordExpiryDays = info.PasswordExpiryDays;
                dbInfo.FailedLoginLockoutAttempts = info.FailedLoginLockoutAttempts;
                dbInfo.RequireUppercase = info.RequireUppercase;
                dbInfo.RequireLowercase = info.RequireLowercase;
                dbInfo.RequireDigit = info.RequireDigit;
                dbInfo.RequireSpecialChar = info.RequireSpecialChar;
                dbInfo.DefaultRoleId = info.DefaultRoleId;
            });
        }

        public async Task SaveDefaultAccountsAsync(CompanyInfo info)
        {
            await SaveInfoAsync(dbInfo =>
            {
                dbInfo.DefaultSalesAccountId = info.DefaultSalesAccountId;
                dbInfo.DefaultAccountsReceivableAccountId = info.DefaultAccountsReceivableAccountId;
                dbInfo.DefaultInventoryAccountId = info.DefaultInventoryAccountId;
                dbInfo.DefaultCogsAccountId = info.DefaultCogsAccountId;
                dbInfo.DefaultPurchasesAccountId = info.DefaultPurchasesAccountId;
                dbInfo.DefaultAccountsPayableAccountId = info.DefaultAccountsPayableAccountId;
                dbInfo.DefaultGoodReceiptsAccrualAccountId = info.DefaultGoodReceiptsAccrualAccountId;
                dbInfo.DefaultCashAccountId = info.DefaultCashAccountId;
                dbInfo.DefaultPurchaseReturnsAccountId = info.DefaultPurchaseReturnsAccountId;
                dbInfo.DefaultPayrollExpenseAccountId = info.DefaultPayrollExpenseAccountId;
                dbInfo.DefaultPayrollAccrualAccountId = info.DefaultPayrollAccrualAccountId;
                dbInfo.DefaultVatAccountId = info.DefaultVatAccountId;
                dbInfo.DefaultInventoryAdjustmentAccountId = info.DefaultInventoryAdjustmentAccountId;
                // *** بداية الإضافة: حفظ حساب مردودات المبيعات ***
                dbInfo.DefaultSalesReturnsAccountId = info.DefaultSalesReturnsAccountId;
                // إضافة الحقل الجديد هنا
                dbInfo.DefaultWipAccountId = info.DefaultWipAccountId;

                // *** نهاية الإضافة ***
            });
        }

        public async Task SaveInventorySettingsAsync(CompanyInfo info)
        {
            await SaveInfoAsync(dbInfo =>
            {
                dbInfo.DefaultCostingMethod = info.DefaultCostingMethod;
            });
        }

        public async Task SaveBackupSettingsAsync(CompanyInfo info)
        {
            await SaveInfoAsync(dbInfo =>
            {
                dbInfo.IsAutoBackupEnabled = info.IsAutoBackupEnabled;
                dbInfo.BackupsToKeep = info.BackupsToKeep;
            });
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Accounts.AsNoTracking().OrderBy(a => a.AccountNumber).ToListAsync();
            }
        }

        public async Task<List<Currency>> GetActiveCurrenciesAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Currencies.AsNoTracking().Where(c => c.IsActive).ToListAsync();
            }
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Roles.AsNoTracking().ToListAsync();
            }
        }

        public async Task<List<NumberingSequence>> GetNumberingSequencesAsync()
        {
            using (var db = new DatabaseContext())
            {
                var settings = await db.NumberingSequences.AsNoTracking().ToListAsync();
                var allDocTypes = Enum.GetValues(typeof(DocumentType)).Cast<DocumentType>();
                var sequencesToDisplay = new List<NumberingSequence>();
                foreach (var docType in allDocTypes)
                {
                    var setting = settings.FirstOrDefault(s => s.DocumentType == docType);
                    if (setting == null)
                    {
                        sequencesToDisplay.Add(new NumberingSequence { DocumentType = docType, LastNumber = 0, NumberOfDigits = 4 });
                    }
                    else
                    {
                        sequencesToDisplay.Add(setting);
                    }
                }
                return sequencesToDisplay.OrderBy(s => s.DocumentType.ToString()).ToList();
            }
        }

        public async Task SaveNumberingSequencesAsync(IEnumerable<NumberingSequence> sequences)
        {
            using (var db = new DatabaseContext())
            {
                foreach (var seq in sequences)
                {
                    var seqInDb = await db.NumberingSequences.FirstOrDefaultAsync(s => s.DocumentType == seq.DocumentType);
                    if (seqInDb != null)
                    {
                        db.Entry(seqInDb).CurrentValues.SetValues(seq);
                    }
                    else
                    {
                        db.NumberingSequences.Add(seq);
                    }
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<NotificationSetting>> GetNotificationSettingsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.NotificationSettings.AsNoTracking().ToListAsync();
            }
        }

        public async Task SaveNotificationSettingsAsync(IEnumerable<NotificationSetting> settings)
        {
            using (var db = new DatabaseContext())
            {
                foreach (var setting in settings)
                {
                    var existing = await db.NotificationSettings.FindAsync(setting.Id);
                    if (existing != null)
                    {
                        db.Entry(existing).CurrentValues.SetValues(setting);
                    }
                    else
                    {
                        db.NotificationSettings.Add(setting);
                    }
                }
                await db.SaveChangesAsync();
            }
        }

        public Task<List<BackupFileViewModel>> GetBackupFilesAsync()
        {
            return Task.Run(() =>
            {
                return Directory.GetFiles(_backupFolder, "*.db")
                    .Select(path => new FileInfo(path))
                    .Select(fi => new BackupFileViewModel
                    {
                        FileName = fi.Name,
                        FilePath = fi.FullName,
                        CreationDate = fi.CreationTime,
                        FileSize = $"{fi.Length / 1024:N0} KB"
                    })
                    .OrderByDescending(f => f.CreationDate).ToList();
            });
        }

        public Task CreateBackupAsync()
        {
            return Task.Run(() =>
            {
                string backupFileName = $"backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db";
                string backupFilePath = Path.Combine(_backupFolder, backupFileName);
                File.Copy(_dbPath, backupFilePath, true);
            });
        }

        public Task RestoreBackupAsync(string backupFilePath)
        {
            return Task.Run(() => File.Copy(backupFilePath, _dbPath, true));
        }

        public Task DeleteBackupAsync(string backupFilePath)
        {
            return Task.Run(() => File.Delete(backupFilePath));
        }
    }
}