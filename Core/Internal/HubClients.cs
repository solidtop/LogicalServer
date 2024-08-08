
namespace LS.Core.Internal
{
    internal sealed class HubClients<THub>(HubManager<THub> hubManager) : IHubClients where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;

        public IClientProxy All { get; } = new AllClientsProxy<THub>(hubManager);

        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds)
        {
            return new AllClientsExceptProxy<THub>(_hubManager, excludedConnectionIds);
        }

        public IClientProxy Client(string connectionId)
        {
            return new SingleClientProxy<THub>(_hubManager, connectionId);
        }

        public IClientProxy Clients(IReadOnlyList<string> connectionIds)
        {
            return new MultipleClientProxy<THub>(_hubManager, connectionIds);
        }
    }
}
