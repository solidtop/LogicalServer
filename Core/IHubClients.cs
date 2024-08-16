namespace LogicalServer.Core
{
    public interface IHubClients
    {
        IClientProxy All { get; }
        IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds);
        IClientProxy Client(string connectionId);
        IClientProxy Clients(IReadOnlyList<string> connectionIds);
        IClientProxy Sessions(IReadOnlyList<string> sessionIds);
        IClientProxy Session(string sessionId);
        IClientProxy SessionExcept(string sessionId, IReadOnlyList<string> excludedConnectionIds);
    }
}
