using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using GoodMorningFactory.UI.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العمليات المتعلقة بالأدوار والصلاحيات.
    /// </summary>
    public interface IRoleService
    {
        Task<PaginatedResult<RoleViewModel>> GetRolesAsync(string searchText, int page, int pageSize);
        Task<Role> GetRoleByIdAsync(int roleId);
        Task AddRoleAsync(Role role);
        Task UpdateRoleAsync(Role role);
        Task DeleteRoleAsync(int roleId);
        Task<ObservableCollection<PermissionGroupViewModel>> GetPermissionsForRoleAsync(int roleId);
        Task SavePermissionsForRoleAsync(int roleId, IEnumerable<PermissionGroupViewModel> permissionGroups);
        Task<List<Role>> GetRolesForCopyingAsync(int currentRoleId);
    }
}
