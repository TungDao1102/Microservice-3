using CatalogService.Dtos;
using CatalogService.Entities;

namespace CatalogService.Extensions
{
    public static class ItemExtension
    {
        public static ItemDto AsDto(this Item item)
        {
            return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
        }
    }
}
