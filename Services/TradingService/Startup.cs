using System.Reflection;
using System.Text.Json.Serialization;
using BuildingBlocks.Common.Contracts;
using BuildingBlocks.Common.HealthChecks;
using BuildingBlocks.Common.Identity;
using BuildingBlocks.Common.MassTransit;
using BuildingBlocks.Common.MongoDB;
using BuildingBlocks.Common.Settings;
using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Models;
using TradingService.Entities;
using TradingService.Exceptions;
using TradingService.SignalR;
using TradingService.StateMachines;

namespace TradingService
{
    public class Startup(IConfiguration configuration)
    {
        private const string AllowedOriginSetting = "AllowedOrigin";
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo(configuration)
                    .AddMongoRepository<CatalogItem>("catalogitems")
                    .AddMongoRepository<InventoryItem>("inventoryitems")
                    .AddMongoRepository<ApplicationUser>("users")
                    .AddJwtBearerAuthentication();

            AddMassTransit(services);

            services.AddSingleton<IUserIdProvider, UserIdProvider>()
                  .AddSingleton<MessageHub>()
                  .AddSignalR();

            services.AddHealthChecks().AddMongoDbCheck(configuration);

            services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            })
            .AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull); // for do not return null values in response

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trading.Service", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Trading.Service v1"));

                app.UseCors(builder =>
                {
                    builder.WithOrigins(configuration[AllowedOriginSetting]!)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MessageHub>("/messagehub");
                endpoints.MapCustomHealthChecks();
            });
        }

        private void AddMassTransit(IServiceCollection services)
        {
            ServiceSettings? serviceSettings = configuration.GetSection(nameof(ServiceSettings))
                                                  .Get<ServiceSettings>();
            MongoDbSettings? mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings))
                                                  .Get<MongoDbSettings>();
            services.AddMassTransit(configure =>
            {
                configure.UsingMessageBroker(configuration, retryConfigurator =>
                    {
                        retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                        retryConfigurator.Ignore(typeof(UnknownItemException));
                    });
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfigurator =>
                    {
                        // only send outbox messages when the saga is completed
                        sagaConfigurator.UseInMemoryOutbox();
                    })
                    .MongoDbRepository(x =>
                    {
                        x.Connection = mongoDbSettings?.ConnectionString;
                        x.DatabaseName = serviceSettings?.ServiceName;
                    });
            });

            var queueSettings = configuration.GetSection(nameof(QueueSettings)).Get<QueueSettings>() ?? throw new ArgumentNullException("Queue settings cannot be null");

            // for using Send in state machine or ISendEndpointProvider 
            EndpointConvention.Map<GrantItems>(new Uri(queueSettings.GrantItemsQueueAddress));
            EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGilQueueAddress));
            EndpointConvention.Map<SubtractItems>(new Uri(queueSettings.SubtractItemsQueueAddress));
        }
    }
}
