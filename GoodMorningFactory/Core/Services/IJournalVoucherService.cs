using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IJournalVoucherService
    {
        Task<List<JournalVoucherViewModel>> GetVouchersAsync();
        Task ReverseVoucherAsync(int voucherId);

        // --- بداية التعديل: تعديل نوع البيانات المرجعة ---
        Task<AddEditJournalVoucherDto> GetInitialDataForAddEditWindowAsync();
        // --- نهاية التعديل ---

        Task SaveVoucherAsync(JournalVoucher voucher);
    }

    public class AddEditJournalVoucherDto
    {
        // --- بداية التعديل: تعديل نوع القائمة ---
        public List<AccountViewModel> Accounts { get; set; }
        // --- نهاية التعديل ---
        public List<CostCenter> CostCenters { get; set; }
    }
}