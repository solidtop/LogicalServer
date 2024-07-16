using LogicalServer.Common.Exceptions;
using LogicalServer.Hubs;
using LogicalServer.Sessions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LogicalServer.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogicalServer(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddHostedService<Worker>();
            services.TryAddSingleton<Server>();
            services.TryAddSingleton<HubManager>();
            services.TryAddSingleton<HubClientStore>();
            services.TryAddSingleton<SessionManager>();
            services.TryAddSingleton<SessionStore>();
            services.TryAddSingleton<IExceptionHandler, ExceptionHandler>();

            return services;
        }

        public static IServiceCollection AddServerOptions(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.Configure<ServerOptions>(configuration.GetSection(nameof(ServerOptions)));
            return services;
        }

        public static IServiceCollection AddSessionFinder(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<SessionFinder>();

            return services;
        }
    }
}
