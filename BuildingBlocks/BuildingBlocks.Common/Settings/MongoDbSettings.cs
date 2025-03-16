namespace BuildingBlocks.Common.Settings
{
    public class MongoDbSettings
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string ConnectionString => $"mongodb://{UserName}:{Password}@{Host}:{Port}";
    }
}
