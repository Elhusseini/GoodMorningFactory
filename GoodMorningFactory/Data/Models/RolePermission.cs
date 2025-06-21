// Data/Models/RolePermission.cs
// *** ملف جديد: يمثل جدول الربط بين الأدوار والصلاحيات ***
namespace GoodMorningFactory.Data.Models
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; }
    }
}