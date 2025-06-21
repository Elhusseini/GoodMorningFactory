// Core/Helpers/PasswordHelper.cs
// *** ملف جديد: فئة مساعدة للتعامل مع كلمات المرور بأمان ***
using System.Security.Cryptography;
using System.Text;

namespace GoodMorningFactory.Core.Helpers
{
    public static class PasswordHelper
    {
        // دالة لتشفير كلمة المرور باستخدام خوارزمية SHA256
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // تحويل كلمة المرور إلى مصفوفة بايت
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                // تحويل مصفوفة البايت المشفرة إلى نص سداسي عشري (hexadecimal)
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // دالة للتحقق من تطابق كلمة المرور المدخلة مع الهاش المخزن
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // نقوم بتشفير كلمة المرور المدخلة بنفس الطريقة
            string hashOfInput = HashPassword(password);
            // نقارن بين الهاش الجديد والهاش المخزن في قاعدة البيانات
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hashedPassword) == 0;
        }
    }
}