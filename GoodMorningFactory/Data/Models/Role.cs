// Data/Models/Role.cs
// *** تحديث: تمت إزالة علاقة مع جدول المستخدمين إذا لم يكن هناك حاجة مباشرة لها ***
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class Role
    {
        public Role()
        {
            RolePermissions = new HashSet<RolePermission>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}