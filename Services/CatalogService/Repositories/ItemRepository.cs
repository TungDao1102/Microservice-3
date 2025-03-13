using CatalogService.Data;
using CatalogService.Entities;
using MongoDB.Driver;

namespace CatalogService.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private const string collectionName = "items";
        private readonly IMongoCollection<Item> _itemCollection;
        private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;
        public ItemRepository(MongoDbService mongoDbService)
        {
            _itemCollection = mongoDbService.Database.GetCollection<Item>(collectionName);
        }

        public async Task<IReadOnlyCollection<Item>> GetAllItemAsync()
        {
            return await _itemCollection.Find(filterBuilder.Empty).ToListAsync();
        }

        public async Task<Item> GetItemAsync(Guid itemId)
        {
            FilterDefinition<Item> filter = filterBuilder.Eq(item => item.Id, itemId);
            return await _itemCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateItemAsync(Item item)
        {
            await _itemCollection.InsertOneAsync(item);
        }

        public async Task UpdateItemAsync(Item item)
        {
            FilterDefinition<Item> filter = filterBuilder.Eq(existingItem => existingItem.Id, item.Id);
            await _itemCollection.ReplaceOneAsync(filter, item);
        }

        public async Task DeleteItemAsync(Guid itemId)
        {
            FilterDefinition<Item> filter = filterBuilder.Eq(item => item.Id, itemId);
            await _itemCollection.DeleteOneAsync(filter);
        }
    }
}
