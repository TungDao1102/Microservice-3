namespace BuildingBlocks.Common.Contracts
{
    public record GrandItems(Guid UserId, Guid CatalogItemId, int Quantity, Guid CorrelationId);

    public record InventoryItemsGranted(Guid CorrelationId);

    public record SubtractItems(Guid UserId, Guid CatalogItemId, int Quantity, Guid CorrelationId);

    public record InventoryItemsSubtracted(Guid CorrelationId);

    public record InventoryItemUpdated(Guid UserId, Guid CatalogItemId, int NewTotalQuantity
   );
}
