using BuildingBlocks.Common.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Common.Logging
{
    public static class Extension
    {
        public static IServiceCollection AddSeqLogging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
            {
                var seqSettings = configuration.GetSection(nameof(SeqSettings)).Get<SeqSettings>();
                loggingBuilder.AddSeq(serverUrl: seqSettings?.ServerUrl);
            });

            return services;
        }
    }
}
