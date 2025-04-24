using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace BuildingBlocks.Common.HealthChecks
{
    public class MongoDbHealthCheck(MongoClient mongoClient) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await mongoClient.ListDatabaseNamesAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(exception: ex);
            }
        }
    }
}
