using BuildingBlocks.Common.Settings;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Common.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQSettings"));
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsingRabbitMq((context, config) =>
                {
                    config.Host(configuration["RabbitMQSettings:Host"]);
                    config.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(configuration["ServiceSettings:ServiceName"], false));
                    config.UseMessageRetry(retryConfig =>
                    {
                        retryConfig.Interval(3, TimeSpan.FromSeconds(5));
                    });
                });
            });
            // use package version 7.1.3
            services.AddMassTransitHostedService();

            return services;
        }
    }
}
