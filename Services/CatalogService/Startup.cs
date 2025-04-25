using BuildingBlocks.Common.HealthChecks;
using BuildingBlocks.Common.Identity;
using BuildingBlocks.Common.MassTransit;
using BuildingBlocks.Common.MongoDB;
using BuildingBlocks.Common.OpenTelemetry;
using CatalogService.Entities;
using Microsoft.OpenApi.Models;

namespace CatalogService
{
    public class Startup(IConfiguration configuration)
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo(configuration)
                .AddMongoRepository<Item>("Items")
                .AddMassTransitWithMessageBroker(configuration);

            //services.AddSingleton<MongoDbService>();
            //services.AddScoped<IItemRepository, ItemRepository>();

            services.AddJwtBearerAuthentication();

            services.AddHealthChecks()
                .AddMongoDbCheck(configuration);

            services.AddTracingAndMetrics(configuration);

            services.AddControllers(options =>
            {
                // for do not remove async suffix in action names (controller)
                options.SuppressAsyncSuffixInActionNames = false;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CatalogService", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CatalogService v1"));
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapCustomHealthChecks();
            });
        }
    }
}
