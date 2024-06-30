using LogicalServer.Common.Exceptions;
using LogicalServer.Session;

namespace LogicalServer.Hubs
{
    public class HubManager
    {
        private readonly HubClientStore _clientStore;
        private readonly SessionManager _sessionManager;
        private readonly ILogger<HubManager> _logger;
        private readonly Dictionary<string, Hub> _hubs = [];

        public HubManager(
            HubClientStore clientStore,
            SessionManager sessionManager,
            IEnumerable<Hub> hubs,
            ILogger<HubManager> logger
            )
        {
            _clientStore = clientStore;
            _sessionManager = sessionManager;
            _logger = logger;

            foreach (var hub in hubs)
            {
                var hubName = hub.GetType().Name.ToLower();
                _hubs[hubName] = hub;
            }
        }

        public async Task RouteMessageAsync(HubMessage message, HubClient client)
        {
            foreach (var hub in _hubs.Values)
            {

                if (hub.Route.Equals(message.Route, StringComparison.OrdinalIgnoreCase))
                {
                    hub.Context = new HubContext(client.Id);

                    await hub.Invoke(message.MethodName, message.Data);
                    return;
                }
            }

            throw new RouteNotFoundException(message.Route);
        }

        public Task OnConnectedAsync()
        {
            _hubs.Values.ToList().ForEach(hub =>
            {
                hub.Clients = new HubClients(_clientStore, hub.Route, _sessionManager);
                hub.OnConnectedAsync();
            });

            return Task.CompletedTask;
        }

        public Task OnDisconnectedAsync(string clientId, Exception? ex)
        {
            _hubs.Values.ToList().ForEach(hub =>
            {
                hub.Clients = new HubClients(_clientStore, hub.Route, _sessionManager);
                hub.OnDisconnectedAsync(ex);
            });

            var session = _sessionManager.FindSession(clientId);

            if (session != null)
            {
                _sessionManager.RemoveFromSessionAsync(clientId, session.Id);
            }

            return Task.CompletedTask;
        }
    }
}
