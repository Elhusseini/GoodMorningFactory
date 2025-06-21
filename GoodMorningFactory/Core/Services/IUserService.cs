using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using GoodMorningFactory.UI.Views;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العمليات المتعلقة بالمستخدمين.
    /// </summary>
    public interface IUserService
    {
        Task<PaginatedResult<UserViewModel>> GetUsersAsync(UserFilterCriteria criteria);
        Task<User> GetUserByIdAsync(int userId);
        Task ToggleUserStatusAsync(int userId);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<List<Role>> GetRolesAsync();
        Task<List<Department>> GetDepartmentsAsync();
        Task<bool> IsUsernameTakenAsync(string username, int? userId = null);
        Task<bool> IsEmailTakenAsync(string email, int? userId = null);
    }
}
