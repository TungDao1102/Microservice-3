using CatalogService.Data;
using CatalogService.Repositories;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace CatalogService
{
    public class Startup(IConfiguration configuration)
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // to use Guid as string in MongoDB instead of binary
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            // use MongoDbService instead
            //var serviceSettings = configuration.GetSection("ServiceSettings").Get<ServiceSettings>();
            //services.AddSingleton(ServiceProvider =>
            //{
            //    var mongoDbSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>() ;
            //    var mongoClient = new MongoClient(mongoDbSettings?.ConnectionString);
            //    return mongoClient.GetDatabase(serviceSettings?.ServiceName);
            //});

            services.AddSingleton<MongoDbService>();
            services.AddScoped<IItemRepository, ItemRepository>();

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
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
