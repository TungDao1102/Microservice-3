using BuildingBlocks.Common.Settings;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Common.MassTransit
{
    public static class Extensions
    {
        private const string RabbitMq = "RABBITMQ";
        private const string ServiceBus = "SERVICEBUS";
        #region IServiceCollection Extensions
        public static IServiceCollection AddMassTransitWithMessageBroker(this IServiceCollection services, IConfiguration configuration, Action<IRetryConfigurator>? configureRetries = null)
        {
            //services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQSettings"));
            //services.AddMassTransit(configure =>
            //{
            //    configure.AddConsumers(Assembly.GetEntryAssembly());
            //    configure.UsingRabbitMq((context, config) =>
            //    {
            //        config.Host(configuration["RabbitMQSettings:Host"]);
            //        config.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(configuration["ServiceSettings:ServiceName"], false));

            //        if (configureRetries is null)
            //        {
            //            configureRetries = (retryConfigurator) =>
            //            {
            //                retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            //            };
            //        }

            //        config.UseMessageRetry(configureRetries);
            //    });
            //});

            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

            switch (serviceSettings?.MessageBroker?.ToUpper())
            {
                case ServiceBus:
                    services.AddMassTransitWithServiceBus(configureRetries);
                    break;
                case RabbitMq:
                default:
                    services.AddMassTransitWithRabbitMq(configureRetries);
                    break;
            }

            // use package version 7.1.3
            //services.AddMassTransitHostedService();

            return services;
        }

        private static IServiceCollection AddMassTransitWithServiceBus(
          this IServiceCollection services,
          Action<IRetryConfigurator>? configureRetries = null)
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsingAzureServiceBusConfig(configureRetries);
            });

            return services;
        }

        private static IServiceCollection AddMassTransitWithRabbitMq(
           this IServiceCollection services,
           Action<IRetryConfigurator>? configureRetries = null)
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsingRabbitMqConfig(configureRetries);
            });

            return services;
        }
        #endregion

        #region IBusRegistrationConfigurator Extensions
        public static void UsingMessageBroker(this IBusRegistrationConfigurator configure, IConfiguration config, Action<IRetryConfigurator>? configureRetries = null)
        {
            var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

            switch (serviceSettings?.MessageBroker?.ToUpper())
            {
                case ServiceBus:
                    configure.UsingAzureServiceBusConfig(configureRetries);
                    break;
                case RabbitMq:
                default:
                    configure.UsingRabbitMqConfig(configureRetries);
                    break;
            }
        }

        private static void UsingRabbitMqConfig(
            this IBusRegistrationConfigurator configure,
            Action<IRetryConfigurator>? configureRetries = null)
        {
            configure.UsingRabbitMq((context, configurator) =>
            {
                var configuration = context.GetService<IConfiguration>();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                configurator.Host(rabbitMQSettings?.Host);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings?.ServiceName, false));

                if (configureRetries == null)
                {
                    configureRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                }

                configurator.UseMessageRetry(configureRetries);
            });
        }

        private static void UsingAzureServiceBusConfig(
           this IBusRegistrationConfigurator configure,
           Action<IRetryConfigurator>? configureRetries = null)
        {
            configure.UsingAzureServiceBus((context, configurator) =>
            {
                var configuration = context.GetService<IConfiguration>();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var serviceBusSettings = configuration.GetSection(nameof(ServiceBusSettings)).Get<ServiceBusSettings>();
                configurator.Host(serviceBusSettings?.ConnectionString);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings?.ServiceName, false));

                if (configureRetries == null)
                {
                    configureRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                }

                configurator.UseMessageRetry(configureRetries);
            });
        }
        #endregion
    }
}
