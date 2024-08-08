using LS.Common.Json;
using LS.Common.Messaging;
using LS.Core;
using LS.Core.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LS.Configuration
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddLogicalServer(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddHostedService<ServerHostedService>();
            services.TryAddSingleton<Server>();

            services.TryAddSingleton<HubMessageParser>();
            services.TryAddSingleton<JsonNamingStrategyProvider>();
            services.TryAddSingleton<HubConnectionStore>();
            services.TryAddSingleton<ConnectionHandlerResolver>();
            services.TryAddSingleton(typeof(HubManager<>));
            services.TryAddSingleton(typeof(HubConnectionHandler<>));
            services.TryAddSingleton(typeof(IHubContext<>), typeof(HubContext<>));
            services.TryAddSingleton(typeof(IHubDispatcher<>), typeof(HubDispatcher<>));
            services.TryAddSingleton(typeof(HubMethodInvoker<>));

            services.TryAddScoped(typeof(HubActivator<>));

            return services;
        }

        public static IServiceCollection AddLogicalServer(this IServiceCollection services, Action<ServerOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddLogicalServer();
            services.Configure(configure);

            return services;
        }

        public static IServiceCollection AddServerOptions(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.Configure<ServerOptions>(configuration.GetSection(nameof(ServerOptions)));

            return services;
        }
    }
}
