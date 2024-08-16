namespace LogicalServer.Core
{
    public interface IHubCallerClients : IHubClients
    {
        IClientProxy Caller { get; }
        IClientProxy Others { get; }
    }
}
