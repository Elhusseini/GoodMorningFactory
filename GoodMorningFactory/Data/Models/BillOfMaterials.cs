// Data/Models/BillOfMaterials.cs
// *** ملف جديد: يمثل رأس قائمة مكونات المنتج ***
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class BillOfMaterials
    {
        public BillOfMaterials()
        {
            BillOfMaterialsItems = new HashSet<BillOfMaterialsItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public int FinishedGoodId { get; set; } // المنتج النهائي
        public virtual Product FinishedGood { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<BillOfMaterialsItem> BillOfMaterialsItems { get; set; }
    }
}