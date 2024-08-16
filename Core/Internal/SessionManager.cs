namespace LogicalServer.Core.Internal
{
    internal sealed class SessionManager<THub>(HubManager<THub> hubManager) : ISessionManager where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;

        public Task AddToSessionAsync(string connectionId, string sessionId, CancellationToken cancellationToken = default)
        {
            return _hubManager.AddToSessionAsync(connectionId, sessionId, cancellationToken);
        }

        public Task RemoveFromSessionAsync(string connectionId, string sessionId, CancellationToken cancellationToken = default)
        {
            return _hubManager.RemoveFromSessionAsync(connectionId, sessionId, cancellationToken);
        }
    }
}
