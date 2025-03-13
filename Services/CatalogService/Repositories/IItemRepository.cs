using CatalogService.Entities;

namespace CatalogService.Repositories
{
    public interface IItemRepository
    {
        Task<IReadOnlyCollection<Item>> GetAllItemAsync();
        Task<Item> GetItemAsync(Guid itemId);
        Task CreateItemAsync(Item item);
        Task UpdateItemAsync(Item item);
        Task DeleteItemAsync(Guid itemId);
    }
}
