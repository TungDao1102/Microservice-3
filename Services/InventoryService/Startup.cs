﻿using BuildingBlocks.Common.MongoDB;
using InventoryService.Clients;
using InventoryService.Entities;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Timeout;

namespace InventoryService
{
    public class Startup(IConfiguration configuration)
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo(configuration)
                .AddMongoRepository<InventoryItem>("InventoryItems");

            AddCatalogClient(services);

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

        private static void AddCatalogClient(IServiceCollection services)
        {
            // to avoid thundering herd problem
            Random jitter = new();

            services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001");
            })
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 1000)),
                onRetry: (outcome, timespan, retryAttempt) =>
                {
                    Console.WriteLine($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
                }))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3, TimeSpan.FromSeconds(15),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"Opening the circuit in {timespan.TotalSeconds} second for {outcome}");
                },
                onReset: () =>
                {
                    Console.WriteLine($"Closing the circuit...");
                }))
             .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1)); // return time out if request time is more than 1 second
        }
    }
}
