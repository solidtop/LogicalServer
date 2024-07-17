using LogicalServer.Hubs;
using LogicalServer.Sessions;

namespace LogicalServer.Configuration
{
    public static class HostApplicationBuilderExtensions
    {
        public static HostApplicationBuilder MapHub<THub>(this HostApplicationBuilder builder, string route) where THub : Hub
        {
            builder.Services.AddSingleton<Hub, THub>(provider =>
            {
                var hub = ActivatorUtilities.CreateInstance<THub>(provider);
                hub.Route = route;
                hub.Sessions = provider.GetRequiredService<SessionManager>();

                return hub;
            });

            return builder;
        }
    }
}
