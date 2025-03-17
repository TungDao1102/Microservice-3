using InventoryService.Dtos;
using InventoryService.Entities;

namespace InventoryService.Extensions
{
    public static class InventoryExtension
    {
        public static InventoryItemDto AsDto(this InventoryItem item, string name, string description)
        {
            return new InventoryItemDto(item.CatalogItemId, name, description, item.Quantity, item.AcquiredDate);
        }
    }
}
