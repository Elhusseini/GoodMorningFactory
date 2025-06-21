// Data/Models/Permission.cs
// *** ملف جديد: يمثل صلاحية واحدة في النظام ***
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
        public string Name { get; set; } // اسم الصلاحية (مثال: Sales.Customers.Create)

        [Required]
        [MaxLength(100)]
        public string Module { get; set; } // اسم الوحدة (مثال: المبيعات)

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}