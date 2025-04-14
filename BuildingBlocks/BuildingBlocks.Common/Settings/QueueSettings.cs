namespace BuildingBlocks.Common.Settings
{
    public class QueueSettings
    {
        public string GrantItemsQueueAddress { get; init; } = string.Empty;
        public string DebitGilQueueAddress { get; init; } = string.Empty;
        public string SubtractItemsQueueAddress { get; init; } = string.Empty;
    }
}
