using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using InventoryService.Entities;
using MassTransit;

namespace InventoryService.Consumers
{
    public class CatalogItemDeletedConsumer(IRepository<CatalogItem> repository) : IConsumer<CatalogItemDeleted>
    {
        private readonly IRepository<CatalogItem> repository = repository;

        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            var message = context.Message;

            var item = await repository.GetAsync(message.ItemId);

            if (item == null)
            {
                return;
            }

            await repository.RemoveAsync(message.ItemId);
        }
    }
}