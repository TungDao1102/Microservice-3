using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using InventoryService.Entities;
using MassTransit;

namespace InventoryService.Consumers
{
    public class CatalogItemCreatedConsumer(IRepository<CatalogItem> repository) : IConsumer<CatalogItemCreated>
    {
        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            CatalogItemCreated message = context.Message;
            var item = await repository.GetAsync(message.ItemId);
            if (item is not null)
            {
                return;
            }

            item = new CatalogItem
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description
            };

            await repository.CreateAsync(item);
        }
    }
}
