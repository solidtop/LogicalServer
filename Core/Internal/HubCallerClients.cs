namespace LogicalServer.Core.Internal
{
    internal sealed class HubCallerClients(string connectionId, IHubClients hubClients) : IHubCallerClients
    {
        private readonly string _connectionId = connectionId;
        private readonly IHubClients _hubClients = hubClients;

        public IClientProxy Caller => _hubClients.Client(_connectionId);

        public IClientProxy Others => _hubClients.AllExcept([_connectionId]);

        public IClientProxy All => _hubClients.All;

        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds)
        {
            return _hubClients.AllExcept(excludedConnectionIds);
        }

        public IClientProxy Client(string connectionId)
        {
            return _hubClients.Client(connectionId);
        }

        public IClientProxy Clients(IReadOnlyList<string> connectionIds)
        {
            return _hubClients.Clients(connectionIds);
        }

        public IClientProxy Sessions(IReadOnlyList<string> sessionIds)
        {
            return _hubClients.Sessions(sessionIds);
        }

        public IClientProxy Session(string sessionId)
        {
            return _hubClients.Session(sessionId);
        }

        public IClientProxy SessionExcept(string sessionId, IReadOnlyList<string> excludedConnectionIds)
        {
            return _hubClients.SessionExcept(sessionId, excludedConnectionIds);
        }
    }
}
