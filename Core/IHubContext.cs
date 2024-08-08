namespace LS.Core
{
    public interface IHubContext
    {
        IHubClients Clients { get; }
    }

    public interface IHubContext<out THub> where THub : Hub
    {
        IHubClients Clients { get; }
    }
}
