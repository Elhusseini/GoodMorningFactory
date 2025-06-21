// Data/Models/Permission.cs
// *** تحديث: تمت إضافة حقل الوصف (Description) ***
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class Permission
    {
        public Permission()
        {
            RolePermissions = new HashSet<RolePermission>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // اسم الصلاحية البرمجي (مثال: Sales.Customers.Create)

        [Required]
        [MaxLength(100)]
        public string Module { get; set; } // اسم الوحدة (مثال: المبيعات)

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } // الوصف الذي يظهر للمستخدم

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}