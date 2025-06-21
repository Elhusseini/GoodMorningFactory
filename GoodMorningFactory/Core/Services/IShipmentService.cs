// GoodMorningFactory/Core/Services/IShipmentService.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels; // سيبقى لـ DTO
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IShipmentService
    {
        // --- تعديل: الخدمة تُرجع الآن نموذج البيانات الخام ---
        Task<PaginatedResult<Shipment>> GetShipmentsAsync(ShipmentFilterCriteria criteria);

        Task<Shipment> GetShipmentForEditAsync(int shipmentId);
        Task UpdateShipmentDetailsAsync(int shipmentId, string carrier, string trackingNumber);
        Task<Shipment> GetShipmentForPackingSlipAsync(int shipmentId);
        Task<ShipmentCreationDataDto> GetShipmentDataForCreationAsync(int salesOrderId);
        Task CreateShipmentAndInvoiceAsync(int salesOrderId, DateTime shipmentDate, IEnumerable<ShipmentItemViewModel> itemsToShip);
    }
}