using LogicalServer.Core;

namespace LS.Core
{
    public interface IHubContext
    {
        IHubClients Clients { get; }
        ISessionManager Sessions { get; }
    }

    public interface IHubContext<out THub> where THub : Hub
    {
        IHubClients Clients { get; }
        ISessionManager Sessions { get; }
    }
}
