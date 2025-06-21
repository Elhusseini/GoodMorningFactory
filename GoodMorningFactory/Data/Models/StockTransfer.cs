// Data/Models/StockTransfer.cs
// *** تحديث جوهري: تم تغيير الربط من المخازن الرئيسية إلى المواقع الفرعية ***
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum StockTransferStatus
    {
        [Description("مكتمل")]
        Completed,
        [Description("ملغي")]
        Cancelled
    }

    [Table("StockTransfers")]
    public class StockTransfer
    {
        public StockTransfer()
        {
            StockTransferItems = new HashSet<StockTransferItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TransferNumber { get; set; }

        [Required]
        public DateTime TransferDate { get; set; }

        // --- بداية التعديل: إضافة المواقع الفرعية وحذف ربط المخازن ---
        [Required]
        public int SourceStorageLocationId { get; set; }
        public virtual StorageLocation SourceStorageLocation { get; set; }

        [Required]
        public int DestinationStorageLocationId { get; set; }
        public virtual StorageLocation DestinationStorageLocation { get; set; }
        // --- نهاية التعديل ---

        public string Notes { get; set; }

        [Required]
        public StockTransferStatus Status { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<StockTransferItem> StockTransferItems { get; set; }
    }
}
