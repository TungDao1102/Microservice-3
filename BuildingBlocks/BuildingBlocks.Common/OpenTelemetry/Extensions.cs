using BuildingBlocks.Common.MassTransit;
using BuildingBlocks.Common.Settings;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Common.OpenTelemetry
{
    public static class Extensions
    {
        public static IServiceCollection AddTracingAndMetrics(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings))
                                                   .Get<ServiceSettings>();
            var jaegerSettings = configuration.GetSection(nameof(JaegerSettings))
                                                             .Get<JaegerSettings>();
            Console.WriteLine(serviceSettings.ServiceName);

            services.AddOpenTelemetry().WithTracing(config =>
            {
                config.AddSource(serviceSettings?.ServiceName ?? string.Empty)
                .AddSource("MassTransit")
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceSettings?.ServiceName ?? "tung"))
                .AddHttpClientInstrumentation() // for outgoing http requests: http client
                .AddAspNetCoreInstrumentation() // for incoming http requests: aspnetcore
                .AddConsoleExporter() // for console
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri($"http://{jaegerSettings?.Host}:{jaegerSettings?.Port}");
                    options.Protocol = OtlpExportProtocol.Grpc;
                }); // for jaeger
            })
            .WithMetrics(configure =>
            {
                configure.AddMeter(serviceSettings?.ServiceName ?? string.Empty)
                .AddMeter("MassTransit")
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter()
                //.AddPrometheusExporter() in OpenTelemetry.Exporter.Prometheus.AspNetCore but under development
                .AddOtlpExporter(); // use for production
            });
            services.AddConsumeObserver<ConsumeObserver>();
            return services;
        }
    }
}
