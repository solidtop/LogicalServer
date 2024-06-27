using LogicalServer.Common.Exceptions;
using LogicalServer.Session;

namespace LogicalServer.Hubs
{
    public class HubClients(HubClientStore clientStore, string route, SessionManager sessionManager)
    {
        private readonly HubClientStore _clientStore = clientStore;
        private readonly string _route = route;
        private readonly SessionManager _sessionManager = sessionManager;

        public HubClientSender All => new(_clientStore.Clients, _route);

        public HubClientSender AllExcept(IReadOnlyList<string> excludedClientIds)
        {
            var clients = _clientStore.Clients
                .Where(client => !excludedClientIds.Contains(client.Key))
                .ToDictionary(client => client.Key, client => client.Value);

            return new HubClientSender(clients, _route);
        }

        public HubClientSender Client(string clientId)
        {
            var client = _clientStore.Clients[clientId] ?? throw new ClientNotFoundException();
            return new HubClientSender(new Dictionary<string, HubClient> { { clientId, client } }, _route);
        }

        public HubClientSender Session(string sessionId)
        {
            var clients = _sessionManager.GetClientsInSession(sessionId);
            return new HubClientSender(clients, _route);
        }

        public HubClientSender SessionExcept(string sessionId, IReadOnlyList<string> excludedClientIds)
        {
            var clients = _sessionManager.GetClientsInSession(sessionId)
                .Where(client => !excludedClientIds.Contains(client.Key))
                .ToDictionary(client => client.Key, client => client.Value);

            return new HubClientSender(clients, _route);
        }
    }
}
