namespace BuildingBlocks.Common.Contracts
{
    public record PurchaseRequested(Guid UserId, Guid ItemId, int Quantity, Guid CorrelationId);
}
