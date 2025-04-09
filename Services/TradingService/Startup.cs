using System.Text.Json.Serialization;
using BuildingBlocks.Common.Identity;
using BuildingBlocks.Common.MongoDB;
using BuildingBlocks.Common.Settings;
using MassTransit;
using Microsoft.OpenApi.Models;
using TradingService.Entities;
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
                configure.UsingPlayEconomyRabbitMq();
                configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>()
                    .MongoDbRepository(x =>
                    {
                        x.Connection = mongoDbSettings?.ConnectionString;
                        x.DatabaseName = serviceSettings?.ServiceName;
                    });
            });

            services.AddMassTransitHostedService();
        }
    }
}
