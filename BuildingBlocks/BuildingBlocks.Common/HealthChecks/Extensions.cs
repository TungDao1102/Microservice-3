using BuildingBlocks.Common.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace BuildingBlocks.Common.HealthChecks
{
    public static class Extensions
    {
        private const string MongoCheckName = "mongodb";
        private const string ReadyTagName = "ready";
        private const string LiveTagName = "live";
        private const string HealthEndpoint = "health";
        private const int DefaultSeconds = 3;

        public static IHealthChecksBuilder AddMongoDbCheck(this IHealthChecksBuilder builder, IConfiguration configuration, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                    MongoCheckName,
                    serviceProvider =>
                    {
                        var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings))
                                                           .Get<MongoDbSettings>();
                        var mongoClient = new MongoClient(mongoDbSettings?.ConnectionString);
                        return new MongoDbHealthCheck(mongoClient);
                    },
                    HealthStatus.Unhealthy,
                    [ReadyTagName],
                    TimeSpan.FromSeconds(DefaultSeconds)
                ));
        }

        public static void MapCustomHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks($"/{HealthEndpoint}/{ReadyTagName}", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains(ReadyTagName)
            });

            // check liveness (application running => return 200) in k8s
            endpoints.MapHealthChecks($"/{HealthEndpoint}/{LiveTagName}", new HealthCheckOptions()
            {
                Predicate = (check) => false
            });
        }
    }
}
