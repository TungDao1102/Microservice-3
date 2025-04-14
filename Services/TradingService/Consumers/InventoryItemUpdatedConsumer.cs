using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using MassTransit;
using TradingService.Entities;

namespace TradingService.Consumers
{
    public class InventoryItemUpdatedConsumer(IRepository<InventoryItem> repository) : IConsumer<InventoryItemUpdated>
    {
        public async Task Consume(ConsumeContext<InventoryItemUpdated> context)
        {
            var message = context.Message;

            var inventoryItem = await repository.GetAsync(
                item => item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = message.CatalogItemId,
                    UserId = message.UserId,
                    Quantity = message.NewTotalQuantity
                };

                await repository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity = message.NewTotalQuantity;
                await repository.UpdateAsync(inventoryItem);
            }
        }
    }
}