using BuildingBlocks.Common.Abstractions;

namespace TradingService.Entities
{
    public class ApplicationUser : IBaseEntity
    {
        public Guid Id { get; set; }
        public decimal Gil { get; set; }
    }
}