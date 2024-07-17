using LogicalServer.Common.Exceptions;

namespace LogicalServer.Hubs
{
    public class HubManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Hub> _hubs = [];

        public HubManager(IServiceProvider serviceProvider, IEnumerable<Hub> hubs)
        {
            _serviceProvider = serviceProvider;

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

        public Task OnConnectedAsync(string clientId)
        {
            _hubs.Values.ToList().ForEach(hub =>
            {
                hub.Context = new HubContext(clientId);
                hub.Clients = ActivatorUtilities.CreateInstance<HubClients>(_serviceProvider, hub.Route);
                hub.OnConnectedAsync();
            });

            return Task.CompletedTask;
        }

        public Task OnDisconnectedAsync(string clientId, Exception? ex)
        {
            _hubs.Values.ToList().ForEach(hub =>
            {
                hub.Context = new HubContext(clientId);
                hub.Clients = ActivatorUtilities.CreateInstance<HubClients>(_serviceProvider, hub.Route);
                hub.OnDisconnectedAsync(ex);
            });

            return Task.CompletedTask;
        }
    }
}
