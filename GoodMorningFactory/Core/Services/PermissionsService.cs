// Core/Services/PermissionsService.cs
// *** تحديث: تم إضافة منطق تحميل صلاحيات المستخدم والتحقق منها ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GoodMorningFactory.Core.Services
{
    public static class PermissionsService
    {
        private static List<string> _userPermissions = new List<string>();

        /// <summary>
        /// تحميل صلاحيات الدور المحدد وتخزينها.
        /// </summary>
        /// <param name="roleId">هوية الدور الخاص بالمستخدم.</param>
        public static void LoadUserPermissions(int roleId)
        {
            using (var db = new DatabaseContext())
            {
                _userPermissions = db.RolePermissions
                                     .Where(rp => rp.RoleId == roleId)
                                     .Include(rp => rp.Permission)
                                     .Select(rp => rp.Permission.Name)
                                     .ToList();
            }
        }

        /// <summary>
        /// التحقق مما إذا كان المستخدم الحالي يمتلك صلاحية معينة.
        /// </summary>
        /// <param name="permissionName">اسم الصلاحية المطلوب التحقق منها.</param>
        /// <returns>True إذا كان المستخدم يمتلك الصلاحية.</returns>
        public static bool CanAccess(string permissionName)
        {
            // المسؤول يمتلك جميع الصلاحيات دائماً
            if (CurrentUserService.LoggedInUser?.Role?.Name == "مسؤول النظام")
            {
                return true;
            }
            return _userPermissions.Contains(permissionName);
        }
    }
}