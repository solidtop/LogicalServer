﻿using LS.Core;

namespace LS.Configuration
{
    public static class HubEndpointBuilderExtensions
    {
        public static IHost MapHub<THub>(this IHost host) where THub : Hub
        {
            var resolver = host.Services.GetRequiredService<ConnectionHandlerResolver>();
            resolver.AddHandler<THub, HubConnectionHandler<THub>>();

            return host;
        }
    }
}
