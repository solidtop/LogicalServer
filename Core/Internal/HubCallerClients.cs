namespace LS.Core.Internal
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
    }
}
