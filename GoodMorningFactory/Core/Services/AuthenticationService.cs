using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    // الواجهة التي تعرف وظائف خدمة المصادقة
    public interface IAuthenticationService
    {
        /// <summary>
        /// يقوم بالتحقق من بيانات المستخدم وتسجيل دخوله
        /// </summary>
        /// <param name="username">اسم المستخدم</param>
        /// <param name="password">كلمة المرور</param>
        /// <returns>كائن المستخدم في حال نجاح الدخول، وإلا null</returns>
        Task<User> LoginAsync(string username, string password);
    }

    // التنفيذ الفعلي لواجهة خدمة المصادقة
    public class AuthenticationService : IAuthenticationService
    {
        public async Task<User> LoginAsync(string username, string password)
        {
            // التأكد من أن المدخلات ليست فارغة
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            using (var db = new DatabaseContext())
            {
                // البحث عن المستخدم في قاعدة البيانات مع جلب دوره
                var user = await db.Users
                                   .Include(u => u.Role)
                                   .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

                // التحقق من وجود المستخدم، نشاط الحساب، وصحة كلمة المرور
                if (user != null && user.IsActive && PasswordHelper.VerifyPassword(password, user.PasswordHash))
                {
                    // 1. تعيين المستخدم الحالي في الخدمة المركزية
                    CurrentUserService.LoggedInUser = user;

                    // 2. تحميل صلاحيات هذا المستخدم فوراً
                    PermissionsService.LoadUserPermissions(user.RoleId);

                    // إرجاع المستخدم كدليل على نجاح العملية
                    return user;
                }

                // في حالة فشل أي من الشروط السابقة
                return null;
            }
        }
    }
}