namespace InventoryService.Exceptions
{
    [Serializable]
    public class UnknownItemException(Guid itemId) : Exception($"Unknown item '{itemId}'")
    {
        public Guid ItemId { get; } = itemId;
    }
}
