namespace LogicalServer.Core.Internal
{
    internal sealed class HubContext<THub>(HubManager<THub> hubManager) : IHubContext, IHubContext<THub> where THub : Hub
    {
        public IHubClients Clients { get; } = new HubClients<THub>(hubManager);

        public ISessionManager Sessions { get; } = new SessionManager<THub>(hubManager);
    }
}
