using BuildingBlocks.Common.Abstractions;

namespace InventoryService.Entities
{
    public class InventoryItem : IBaseEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CatalogItemId { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset AcquiredDate { get; set; }
        public HashSet<Guid> MessageIds { get; set; } = new();
    }
}
