using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using InventoryService.Entities;
using MassTransit;

namespace InventoryService.Consumers
{
    public class CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository) : IConsumer<CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> repository = repository;

        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            var message = context.Message;

            var item = await repository.GetAsync(message.ItemId);

            if (item == null)
            {
                item = new CatalogItem
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description
                };

                await repository.CreateAsync(item);
            }
            else
            {
                item.Name = message.Name;
                item.Description = message.Description;

                await repository.UpdateAsync(item);
            }
        }
    }
}