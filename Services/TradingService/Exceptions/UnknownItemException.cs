namespace TradingService.Exceptions
{
    [Serializable]
    internal class UnknownItemException(Guid itemId) : Exception($"Unknown item '{itemId}'")
    {
        public Guid ItemId { get; } = itemId;
    }
}