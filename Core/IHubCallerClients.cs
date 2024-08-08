namespace LS.Core
{
    public interface IHubCallerClients : IHubClients
    {
        IClientProxy Caller { get; }
        IClientProxy Others { get; }
    }
}
