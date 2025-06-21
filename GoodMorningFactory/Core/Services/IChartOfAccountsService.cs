// GoodMorningFactory/Core/Services/IChartOfAccountsService.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    // DTO جديد لنقل البيانات اللازمة لنافذة الإضافة/التعديل
    public class AddEditAccountDto
    {
        public Account Account { get; set; }
        public List<Account> AllAccounts { get; set; }
    }

    public interface IChartOfAccountsService
    {
        Task<ObservableCollection<AccountViewModel>> GetAccountTreeAsync();
        Task<List<LedgerEntryViewModel>> GetLedgerEntriesAsync(int accountId);
        Task DeleteAccountAsync(int accountId);

        // --- بداية الإضافة: دوال جديدة ---
        Task<AddEditAccountDto> GetDataForAddEditAccountAsync(int? accountId);
        Task SaveAccountAsync(Account account);
        // --- نهاية الإضافة ---
    }
}