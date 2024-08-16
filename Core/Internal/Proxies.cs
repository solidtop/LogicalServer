namespace LogicalServer.Core.Internal
{
    internal sealed class AllClientsProxy<THub>(HubManager<THub> hubManager) : IClientProxy where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;

        public Task SendCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            return _hubManager.SendAllAsync(methodName, args, cancellationToken);
        }
    }

    internal sealed class AllClientsExceptProxy<THub>(HubManager<THub> hubManager, IReadOnlyList<string> excludedConnectionIds) : IClientProxy where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;
        private readonly IReadOnlyList<string> _excludedConnectionIds = excludedConnectionIds;

        public Task SendCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            return _hubManager.SendAllExceptAsync(methodName, args, _excludedConnectionIds, cancellationToken);
        }
    }

    internal sealed class MultipleClientProxy<THub>(HubManager<THub> hubManager, IReadOnlyList<string> connectionIds) : IClientProxy where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;
        private readonly IReadOnlyList<string> _connectionIds = connectionIds;

        public Task SendCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            return _hubManager.SendConnectionsAsync(methodName, args, _connectionIds, cancellationToken);
        }
    }

    internal sealed class SingleClientProxy<THub>(HubManager<THub> hubManager, string connectionId) : IClientProxy where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;
        private readonly string _connectionId = connectionId;

        public Task SendCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            return _hubManager.SendConnectionAsync(_connectionId, methodName, args, cancellationToken);
        }
    }

    internal sealed class SessionProxy<THub>(HubManager<THub> hubManager, string sessionId) : IClientProxy where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;
        private readonly string _sessionId = sessionId;

        public Task SendCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            return _hubManager.SendSessionAsync(_sessionId, methodName, args, cancellationToken);
        }
    }

    internal sealed class SessionExceptProxy<THub>(HubManager<THub> hubManager, string sessionId, IReadOnlyList<string> excludedConnectionIds) : IClientProxy where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;
        private readonly string _sessionId = sessionId;
        private readonly IReadOnlyList<string> _excludedConnectionIds = excludedConnectionIds;

        public Task SendCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            return _hubManager.SendSessionExcept(_sessionId, methodName, args, _excludedConnectionIds, cancellationToken);
        }
    }

    internal sealed class MultipleSessionProxy<THub>(HubManager<THub> hubManager, IReadOnlyList<string> sessionIds) : IClientProxy where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;
        private readonly IReadOnlyList<string> _sessionIds = sessionIds;

        public Task SendCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            return _hubManager.SendSessionsAsync(_sessionIds, methodName, args, cancellationToken);
        }
    }
}
