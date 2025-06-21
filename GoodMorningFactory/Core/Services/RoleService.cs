using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using GoodMorningFactory.UI.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class RoleService : IRoleService
    {
        public async Task<PaginatedResult<RoleViewModel>> GetRolesAsync(string searchText, int page, int pageSize)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Roles.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(r => r.Name.Contains(searchText) || r.Description.Contains(searchText));
                }

                var totalItems = await query.CountAsync();
                var rolesForPage = await query.OrderBy(r => r.Name)
                                              .Skip((page - 1) * pageSize)
                                              .Take(pageSize)
                                              .ToListAsync();

                var roleIds = rolesForPage.Select(r => r.Id).ToList();
                var userCounts = await db.Users
                                         .Where(u => roleIds.Contains(u.RoleId))
                                         .GroupBy(u => u.RoleId)
                                         .ToDictionaryAsync(g => g.Key, g => g.Count());

                var roleViewModels = rolesForPage.Select(role => new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    UserCount = userCounts.ContainsKey(role.Id) ? userCounts[role.Id] : 0
                }).ToList();

                return new PaginatedResult<RoleViewModel> { Items = roleViewModels, TotalCount = totalItems };
            }
        }

        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Roles.FindAsync(roleId);
            }
        }

        public async Task AddRoleAsync(Role role)
        {
            using (var db = new DatabaseContext())
            {
                db.Roles.Add(role);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateRoleAsync(Role role)
        {
            using (var db = new DatabaseContext())
            {
                db.Roles.Update(role);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteRoleAsync(int roleId)
        {
            using (var db = new DatabaseContext())
            {
                var role = await db.Roles.FindAsync(roleId);
                if (role == null) return;

                if (role.Name == "مسؤول النظام")
                    throw new InvalidOperationException("لا يمكن حذف دور المسؤول الرئيسي.");

                if (await db.Users.AnyAsync(u => u.RoleId == roleId))
                    throw new InvalidOperationException("لا يمكن حذف هذا الدور لوجود مستخدمين مرتبطين به.");

                var permissions = db.RolePermissions.Where(rp => rp.RoleId == roleId);
                db.RolePermissions.RemoveRange(permissions);
                db.Roles.Remove(role);
                await db.SaveChangesAsync();
            }
        }

        public async Task<ObservableCollection<PermissionGroupViewModel>> GetPermissionsForRoleAsync(int roleId)
        {
            using (var db = new DatabaseContext())
            {
                var allPermissions = await db.Permissions.ToListAsync();
                var rolePermissionIds = await db.RolePermissions
                                                .Where(rp => rp.RoleId == roleId)
                                                .Select(rp => rp.PermissionId)
                                                .ToHashSetAsync();

                var permissionGroups = new ObservableCollection<PermissionGroupViewModel>();
                var groupedByModule = allPermissions.GroupBy(p => p.Module);

                foreach (var group in groupedByModule)
                {
                    var groupVm = new PermissionGroupViewModel(group.Key);
                    foreach (var perm in group)
                    {
                        var permVm = new PermissionViewModel
                        {
                            Id = perm.Id,
                            Description = perm.Description,
                            IsSelected = rolePermissionIds.Contains(perm.Id),
                            Parent = groupVm
                        };
                        groupVm.Permissions.Add(permVm);
                    }
                    groupVm.VerifyCheckState();
                    permissionGroups.Add(groupVm);
                }
                return permissionGroups;
            }
        }

        public async Task SavePermissionsForRoleAsync(int roleId, IEnumerable<PermissionGroupViewModel> permissionGroups)
        {
            using (var db = new DatabaseContext())
            {
                var existingPermissions = db.RolePermissions.Where(rp => rp.RoleId == roleId);
                db.RolePermissions.RemoveRange(existingPermissions);

                var newPermissions = permissionGroups
                    .SelectMany(g => g.Permissions)
                    .Where(p => p.IsSelected)
                    .Select(p => new RolePermission { RoleId = roleId, PermissionId = p.Id });

                await db.RolePermissions.AddRangeAsync(newPermissions);
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Role>> GetRolesForCopyingAsync(int currentRoleId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Roles.Where(r => r.Id != currentRoleId).ToListAsync();
            }
        }
    }
}
