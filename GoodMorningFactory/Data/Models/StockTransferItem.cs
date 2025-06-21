// Data/Models/StockTransferItem.cs
// *** ملف جديد: يمثل سطراً واحداً (منتج) في عملية تحويل المخزون ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    [Table("StockTransferItems")]
    public class StockTransferItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StockTransferId { get; set; }
        public virtual StockTransfer StockTransfer { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
