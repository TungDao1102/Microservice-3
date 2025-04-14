using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using MassTransit;
using TradingService.Entities;

namespace TradingService.Consumers
{
    public class CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository) : IConsumer<CatalogItemUpdated>
    {
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
                    Description = message.Description,
                    Price = message.Price
                };

                await repository.CreateAsync(item);
            }
            else
            {
                item.Name = message.Name;
                item.Description = message.Description;
                item.Price = message.Price;

                await repository.UpdateAsync(item);
            }
        }
    }
}