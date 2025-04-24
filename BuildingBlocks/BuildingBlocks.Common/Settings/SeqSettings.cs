namespace BuildingBlocks.Common.Settings
{
    public class SeqSettings
    {
        public string Host { get; init; } = string.Empty;
        public int Port { get; init; }

        public string ServerUrl
        {
            get { return $"http://{Host}:{Port}"; }
        }
    }
}