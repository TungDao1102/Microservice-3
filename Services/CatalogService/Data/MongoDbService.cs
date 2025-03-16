using BuildingBlocks.Common.Settings;
using MongoDB.Driver;

namespace CatalogService.Data
{
    [Obsolete("This interface is obsolete, use AddMongo method instead")]
    public class MongoDbService
    {
        public IMongoDatabase Database { get; } = default!;
        public MongoDbService(IConfiguration configuration)
        {
            var mongoDbSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var serviceSettings = configuration.GetSection("ServiceSettings").Get<ServiceSettings>();
            var mongoClient = new MongoClient(mongoDbSettings?.ConnectionString);
            Database = mongoClient.GetDatabase(serviceSettings?.ServiceName);
        }

    }
}
