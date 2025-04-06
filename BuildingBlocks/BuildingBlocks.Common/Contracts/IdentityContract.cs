namespace BuildingBlocks.Common.Contracts
{
    public record UserUpdated(Guid UserId, string Email, decimal NewTotalGil);

    public record DebitGil(Guid UserId, decimal Gil, Guid CorrelationId);

    public record GilDebited(Guid CorrelationId);
}
