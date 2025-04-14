using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using MassTransit;
using TradingService.Entities;

namespace TradingService.Consumers
{
    public class CatalogItemCreatedConsumer(IRepository<CatalogItem> repository) : IConsumer<CatalogItemCreated>
    {
        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var message = context.Message;

            var item = await repository.GetAsync(message.ItemId);

            if (item != null)
            {
                return;
            }

            item = new CatalogItem
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description,
                Price = message.Price
            };

            await repository.CreateAsync(item);
        }
    }
}