namespace LS.Core
{
    public interface IHubClients
    {
        IClientProxy All { get; }
        IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds);
        IClientProxy Client(string connectionId);
        IClientProxy Clients(IReadOnlyList<string> connectionIds);
    }
}
