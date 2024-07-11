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
                var hub = ActivatorUtilities.CreateInstance<T>(provider);
                hub.Route = route;
                hub.Sessions = provider.GetRequiredService<SessionManager>();

                return hub;
            });

            return builder;
        }
    }
}
