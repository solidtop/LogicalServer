using LogicalServer.Hubs;
using LogicalServer.Session;

namespace LogicalServer.Configuration
{
    public static class HostApplicationBuilderExtensions
    {
        public static HostApplicationBuilder MapHub<T>(this HostApplicationBuilder builder, string route) where T : Hub
        {
            builder.Services.AddSingleton<Hub, T>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<T>>();
                var sessionManager = provider.GetRequiredService<SessionManager>();

                var hub = Activator.CreateInstance(typeof(T), [logger]) as T ?? throw new NullReferenceException("Hub not found");
                hub.Route = route;
                hub.Sessions = sessionManager;

                return hub;
            });

            return builder;
        }
    }
}
