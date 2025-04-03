namespace BuildingBlocks.Common.Settings
{
    public class ServiceSettings
    {
        public string ServiceName { get; init; } = string.Empty;
        public string Authority { get; init; } = string.Empty;
        public string MessageBroker { get; init; } = string.Empty;
        public string KeyVaultName { get; init; } = string.Empty;
    }
}
