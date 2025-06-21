// Data/Models/Role.cs
// *** تحديث: تمت إضافة علاقة مع جدول الصلاحيات ***
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class Role
    {
        public Role()
        {
            Users = new HashSet<User>();
            RolePermissions = new HashSet<RolePermission>(); // <-- إضافة جديدة
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; } // <-- إضافة جديدة
    }
}