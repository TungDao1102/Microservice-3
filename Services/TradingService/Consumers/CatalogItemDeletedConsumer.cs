using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using MassTransit;
using TradingService.Entities;

namespace TradingService.Consumers
{
    public class CatalogItemDeletedConsumer(IRepository<CatalogItem> repository) : IConsumer<CatalogItemDeleted>
    {
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