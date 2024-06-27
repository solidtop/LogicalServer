using LogicalServer.Hubs;
using LogicalServer.Session;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LogicalServer.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogicalServer(this IServiceCollection services)
        {
            services.AddHostedService<Worker>();
            services.TryAddSingleton<Server>();
            services.TryAddSingleton<HubManager>();
            services.TryAddSingleton<HubClientStore>();
            services.TryAddSingleton<SessionManager>();

            return services;
        }

        public static IServiceCollection AddServerOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ServerOptions>(configuration.GetSection(nameof(ServerOptions)));
            return services;
        }
    }
}
