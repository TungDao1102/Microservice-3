using BuildingBlocks.Common.Abstractions;

namespace TradingService.Entities
{
    public class InventoryItem : IBaseEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CatalogItemId { get; set; }
        public int Quantity { get; set; }
    }
}