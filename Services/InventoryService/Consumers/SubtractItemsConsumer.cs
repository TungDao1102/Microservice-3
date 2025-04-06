using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using InventoryService.Entities;
using InventoryService.Exceptions;
using MassTransit;

namespace InventoryService.Consumers
{
    public class SubtractItemsConsumer(
        IRepository<InventoryItem> inventoryItemsRepository,
        IRepository<CatalogItem> catalogItemsRepository) : IConsumer<SubtractItems>
    {
        public async Task Consume(ConsumeContext<SubtractItems> context)
        {
            var message = context.Message;

            var item = await catalogItemsRepository.GetAsync(message.CatalogItemId);

            if (item == null)
            {
                throw new UnknownItemException(message.CatalogItemId);
            }

            var inventoryItem = await inventoryItemsRepository.GetAsync(
                item => item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);

            if (inventoryItem != null)
            {
                if (inventoryItem.MessageIds.Contains(context.MessageId!.Value))
                {
                    await context.Publish(new InventoryItemsSubtracted(message.CorrelationId));
                    return;
                }

                inventoryItem.Quantity -= message.Quantity;
                inventoryItem.MessageIds.Add(context.MessageId!.Value);
                await inventoryItemsRepository.UpdateAsync(inventoryItem);

                await context.Publish(new InventoryItemUpdated(
                    inventoryItem.UserId,
                    inventoryItem.CatalogItemId,
                    inventoryItem.Quantity));
            }

            await context.Publish(new InventoryItemsSubtracted(message.CorrelationId));
        }
    }
}
