using BuildingBlocks.Common.Abstractions;

namespace TradingService.Entities
{
    public class CatalogItem : IBaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}