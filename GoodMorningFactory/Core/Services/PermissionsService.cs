// Core/Services/PermissionsService.cs
// *** تحديث: تم إصلاح منطق التحقق من صلاحيات المدير ليعتمد على اسم المستخدم الثابت "admin" لضمان الموثوقية ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GoodMorningFactory.Core.Services
{
    public static class PermissionsService
    {
        private static HashSet<string> _userPermissions = new HashSet<string>();

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
                                     .ToHashSet();
            }
        }

        /// <summary>
        /// التحقق مما إذا كان المستخدم الحالي يمتلك صلاحية معينة.
        /// </summary>
        /// <param name="permissionName">اسم الصلاحية المطلوب التحقق منها.</param>
        /// <returns>True إذا كان المستخدم يمتلك الصلاحية.</returns>
        public static bool CanAccess(string permissionName)
        {
            // --- بداية الإصلاح الجذري ---
            // التحقق من صلاحيات المدير العام بناءً على اسم المستخدم الثابت "admin"
            // هذا الأسلوب أكثر أماناً من الاعتماد على رقم الدور أو اسمه المتغير.
            if (CurrentUserService.LoggedInUser?.Username.ToLower() == "admin")
            {
                return true;
            }
            // --- نهاية الإصلاح ---

            // التحقق من الصلاحيات الممنوحة لبقية الأدوار
            return _userPermissions.Contains(permissionName);
        }
    }
}
